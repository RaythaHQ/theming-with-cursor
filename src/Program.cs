using System.Text.Json;
using System.Text.RegularExpressions;
using RaythaSimulator;
using RaythaSimulator.Models;

// Parse command line arguments
string? sampleDataFile = null;
string? outputPath = null;
string siteName = "default";
bool renderAll = false;
bool syncOnly = false;
bool forceSync = false;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--output" || args[i] == "-o")
    {
        if (i + 1 < args.Length)
        {
            outputPath = args[++i];
        }
        else
        {
            Console.Error.WriteLine("Error: --output requires a path argument");
            return 1;
        }
    }
    else if (args[i] == "--site")
    {
        if (i + 1 < args.Length)
        {
            siteName = args[++i];
        }
        else
        {
            Console.Error.WriteLine("Error: --site requires a name argument");
            return 1;
        }
    }
    else if (args[i] == "--help" || args[i] == "-h")
    {
        PrintUsage();
        return 0;
    }
    else if (args[i] == "--all" || args[i] == "-a")
    {
        renderAll = true;
    }
    else if (args[i] == "--sync" || args[i] == "-s")
    {
        forceSync = true;
    }
    else if (args[i] == "--sync-only")
    {
        syncOnly = true;
        forceSync = true;
    }
    else if (!args[i].StartsWith('-'))
    {
        sampleDataFile = args[i];
    }
}

// Determine directories
var workingDir = Directory.GetCurrentDirectory();
var projectRoot = FindProjectRoot(workingDir);

// Source directories (starter templates that never change)
var srcDir = Path.Combine(projectRoot, "src");
var srcLiquidDir = Path.Combine(srcDir, "liquid");
var srcModelsDir = Path.Combine(srcDir, "models");

// Project directories (in /dist/<site>/)
var distDir = Path.Combine(projectRoot, "dist");
var siteDir = Path.Combine(distDir, siteName);
var siteLiquidDir = Path.Combine(siteDir, "liquid");
var siteModelsDir = Path.Combine(siteDir, "models");
var siteSampleDataDir = Path.Combine(siteDir, "sample-data");
var siteHtmlOutputDir = outputPath ?? Path.Combine(siteDir, "htmlOutput");

var configPath = Path.Combine(projectRoot, "raytha.config.json");

// Initialize project if it doesn't exist
if (!Directory.Exists(siteDir))
{
    Console.WriteLine($"Initializing new project: {siteName}");
    InitializeProject(siteDir, srcLiquidDir, srcModelsDir, siteLiquidDir, siteModelsDir, siteSampleDataDir, siteHtmlOutputDir);
    Console.WriteLine();
    Console.WriteLine("Project initialized! Next steps:");
    Console.WriteLine($"  1. Add sample data to: dist/{siteName}/sample-data/");
    Console.WriteLine($"  2. Customize templates in: dist/{siteName}/liquid/");
    Console.WriteLine($"  3. Define content models in: dist/{siteName}/models/");
    Console.WriteLine($"  4. Run 'dotnet run -- --site {siteName}' to render");
    Console.WriteLine();
    return 0;
}

// Load Raytha config if present
RaythaConfig? config = null;
if (File.Exists(configPath))
{
    try
    {
        var configJson = File.ReadAllText(configPath);
        config = JsonSerializer.Deserialize<RaythaConfig>(configJson);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Warning: Failed to load raytha.config.json: {ex.Message}");
    }
}

// If sync-only mode, just sync and exit
if (syncOnly)
{
    if (config == null)
    {
        Console.Error.WriteLine("Error: raytha.config.json not found. Cannot sync templates.");
        return 1;
    }

    if (string.IsNullOrEmpty(config.ApiKey) || config.ApiKey == "YOUR_API_KEY_HERE")
    {
        Console.Error.WriteLine("Error: API key not configured in raytha.config.json");
        return 1;
    }

    return await SyncTemplates(config, siteLiquidDir);
}

if (!Directory.Exists(siteLiquidDir))
{
    Console.Error.WriteLine($"Error: Liquid templates directory not found: {siteLiquidDir}");
    Console.Error.WriteLine($"Run 'dotnet run -- --site {siteName}' first to initialize the project.");
    return 1;
}

// Check if sample-data has any files
if (!Directory.Exists(siteSampleDataDir) || !Directory.GetFiles(siteSampleDataDir, "*.json").Any())
{
    Console.Error.WriteLine($"Warning: No sample data found in: {siteSampleDataDir}");
    Console.Error.WriteLine("Add menus.json and site-pages.json (or content type files) to render templates.");
    Console.Error.WriteLine();
}

// Ensure output directory exists
Directory.CreateDirectory(siteHtmlOutputDir);

Console.WriteLine($"=== Rendering Site: {siteName} ===");
Console.WriteLine($"Templates: dist/{siteName}/liquid/");
Console.WriteLine($"Data: dist/{siteName}/sample-data/");
Console.WriteLine($"Output: dist/{siteName}/htmlOutput/");
Console.WriteLine();

// Render templates
int renderResult;
if (renderAll || string.IsNullOrEmpty(sampleDataFile))
{
    renderResult = RenderAllSampleData(siteSampleDataDir, siteLiquidDir, siteHtmlOutputDir);
}
else
{
    var sampleDataPath = Path.GetFullPath(sampleDataFile, workingDir);
    if (!File.Exists(sampleDataPath))
    {
        Console.Error.WriteLine($"Error: Sample data file not found: {sampleDataPath}");
        return 1;
    }
    renderResult = RenderSampleDataFile(sampleDataPath, siteLiquidDir, siteSampleDataDir, siteHtmlOutputDir);
}

// Sync to Raytha if enabled
bool shouldSync = forceSync || (config?.AutoSync ?? false);
if (shouldSync && config != null)
{
    if (string.IsNullOrEmpty(config.ApiKey) || config.ApiKey == "YOUR_API_KEY_HERE")
    {
        Console.WriteLine("\nSkipping sync: API key not configured in raytha.config.json");
    }
    else
    {
        var syncResult = await SyncTemplates(config, siteLiquidDir);
        if (syncResult != 0 && renderResult == 0)
        {
            renderResult = syncResult;
        }
    }
}
else if (forceSync && config == null)
{
    Console.Error.WriteLine("\nWarning: --sync specified but raytha.config.json not found");
}

return renderResult;

static void InitializeProject(string siteDir, string srcLiquidDir, string srcModelsDir,
    string siteLiquidDir, string siteModelsDir, string siteSampleDataDir, string siteHtmlOutputDir)
{
    // Create directory structure
    Directory.CreateDirectory(siteDir);
    Directory.CreateDirectory(siteLiquidDir);
    Directory.CreateDirectory(siteModelsDir);
    Directory.CreateDirectory(siteSampleDataDir);
    Directory.CreateDirectory(siteHtmlOutputDir);

    // Copy liquid templates
    if (Directory.Exists(srcLiquidDir))
    {
        CopyDirectory(srcLiquidDir, siteLiquidDir);
        Console.WriteLine($"  Copied starter templates to liquid/");
    }

    // Copy starter models
    if (Directory.Exists(srcModelsDir))
    {
        CopyDirectory(srcModelsDir, siteModelsDir);
        Console.WriteLine($"  Copied starter models to models/");
    }

    // Create placeholder menus.json
    var menusPath = Path.Combine(siteSampleDataDir, "menus.json");
    var starterMenus = new
    {
        menus = new[]
        {
            new
            {
                Id = "main-menu",
                Label = "Main Menu",
                DeveloperName = "main_menu",
                IsMainMenu = true,
                MenuItems = new[]
                {
                    new { Id = "menu-1", Label = "Home", Url = "/", Ordinal = 1, IsFirstItem = true, IsLastItem = false },
                    new { Id = "menu-2", Label = "About", Url = "/about", Ordinal = 2, IsFirstItem = false, IsLastItem = false },
                    new { Id = "menu-3", Label = "Contact", Url = "/contact", Ordinal = 3, IsFirstItem = false, IsLastItem = true }
                }
            }
        }
    };
    File.WriteAllText(menusPath, JsonSerializer.Serialize(starterMenus, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"  Created starter menus.json");

    Console.WriteLine($"  Created sample-data/ (add your site's data here)");
    Console.WriteLine($"  Created htmlOutput/ (generated HTML will go here)");
}

static void CopyDirectory(string sourceDir, string destDir)
{
    Directory.CreateDirectory(destDir);

    foreach (var file in Directory.GetFiles(sourceDir))
    {
        var destFile = Path.Combine(destDir, Path.GetFileName(file));
        File.Copy(file, destFile, overwrite: true);
    }

    foreach (var dir in Directory.GetDirectories(sourceDir))
    {
        var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
        CopyDirectory(dir, destSubDir);
    }
}

static async Task<int> SyncTemplates(RaythaConfig config, string liquidDir)
{
    try
    {
        using var client = new RaythaApiClient(config, liquidDir);
        var result = await client.SyncAllTemplatesAsync();
        return (result.Failed.Count > 0 || result.WidgetFailed.Count > 0) ? 1 : 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error syncing templates: {ex.Message}");
        return 1;
    }
}

static int RenderAllSampleData(string sampleDataDir, string liquidDir, string htmlDir)
{
    if (!Directory.Exists(sampleDataDir))
    {
        Console.Error.WriteLine($"Error: Sample data directory not found: {sampleDataDir}");
        return 1;
    }

    int successCount = 0;
    int failCount = 0;

    // Reserved files that aren't content type data
    var reservedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "menus.json",
        "site-pages.json"
    };

    // Render Site Pages first
    var sitePagesPath = Path.Combine(sampleDataDir, "site-pages.json");
    if (File.Exists(sitePagesPath))
    {
        Console.WriteLine("--- Site Pages ---");
        var result = RenderSitePages(sitePagesPath, liquidDir, sampleDataDir, htmlDir);
        if (result == 0)
            successCount++;
        else
            failCount++;
        Console.WriteLine();
    }

    // Render Content Type data files
    var jsonFiles = Directory.GetFiles(sampleDataDir, "*.json")
        .Where(f => !reservedFiles.Contains(Path.GetFileName(f)))
        .ToList();

    if (jsonFiles.Count > 0)
    {
        Console.WriteLine("--- Content Types ---");
        Console.WriteLine($"Found {jsonFiles.Count} content type file(s)");
        Console.WriteLine();

        foreach (var jsonFile in jsonFiles)
        {
            var result = RenderSampleDataFile(jsonFile, liquidDir, sampleDataDir, htmlDir);
            if (result == 0)
                successCount++;
            else
                failCount++;
            Console.WriteLine();
        }
    }

    if (successCount == 0 && failCount == 0)
    {
        Console.WriteLine("No data files to render.");
        Console.WriteLine("Add site-pages.json and/or content type JSON files to sample-data/");
        return 0;
    }

    Console.WriteLine($"Rendering complete: {successCount} succeeded, {failCount} failed");
    return failCount > 0 ? 1 : 0;
}

static int RenderSitePages(string sitePagesPath, string liquidDir, string sampleDataDir, string htmlDir)
{
    try
    {
        Console.WriteLine($"Loading: {Path.GetFileName(sitePagesPath)}");
        var json = File.ReadAllText(sitePagesPath);
        var sitePagesData = JsonSerializer.Deserialize<SitePagesData>(json);

        if (sitePagesData?.Pages == null || sitePagesData.Pages.Count == 0)
        {
            Console.WriteLine("  No site pages found");
            return 0;
        }

        var timeZone = sitePagesData.CurrentOrganization?.TimeZone ?? "UTC";
        var renderEngine = new RenderEngine(liquidDir, sampleDataDir, timeZone);

        Console.WriteLine($"  Rendering {sitePagesData.Pages.Count} page(s)...");

        foreach (var page in sitePagesData.Pages)
        {
            try
            {
                RenderSitePage(page, sitePagesData, renderEngine, liquidDir, htmlDir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"    Error: {page.Title} - {ex.Message}");
            }
        }

        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static void RenderSitePage(SitePageModel page, SitePagesData sitePagesData, RenderEngine renderEngine, string liquidDir, string htmlDir)
{
    var templateName = page.WebTemplateDeveloperName;
    if (!templateName.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase))
    {
        templateName += ".liquid";
    }

    var templatePath = Path.Combine(liquidDir, templateName);
    if (!File.Exists(templatePath))
    {
        throw new FileNotFoundException($"Template not found: {templatePath}");
    }

    var templateSource = File.ReadAllText(templatePath);

    // Create context for the site page
    var context = new SimulatorContext
    {
        Target = new Dictionary<string, object?>
        {
            ["Id"] = page.Id,
            ["Title"] = page.Title,
            ["RoutePath"] = page.RoutePath,
            ["IsPublished"] = page.IsPublished,
            ["CreationTime"] = page.CreationTime
        },
        CurrentOrganization = sitePagesData.CurrentOrganization ?? new OrganizationModel { OrganizationName = "Sample Organization" },
        CurrentUser = sitePagesData.CurrentUser ?? new UserModel(),
        PathBase = sitePagesData.PathBase,
        QueryParams = new Dictionary<string, string>()
    };

    // Convert widgets to render data format
    Dictionary<string, List<SitePageWidgetRenderData>>? widgets = null;
    if (page.PublishedWidgets != null)
    {
        widgets = page.PublishedWidgets.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(SitePageWidgetRenderData.FromModel).ToList()
        );
    }

    // Render the page with widgets
    var html = renderEngine.RenderAsHtml(templateSource, context, widgets);

    // Determine output filename
    var outputFileName = string.IsNullOrEmpty(page.RoutePath) || page.RoutePath == "home"
        ? "index.html"
        : $"{page.RoutePath}.html";
    var outputFilePath = Path.Combine(htmlDir, outputFileName);

    File.WriteAllText(outputFilePath, html);
    Console.WriteLine($"    {page.Title} -> {outputFileName}");
}

static int RenderSampleDataFile(string sampleDataPath, string liquidDir, string sampleDataDir, string htmlDir)
{
    try
    {
        Console.WriteLine($"Loading: {Path.GetFileName(sampleDataPath)}");
        var json = File.ReadAllText(sampleDataPath);
        var sampleData = JsonSerializer.Deserialize<SampleData>(json);

        if (sampleData == null)
        {
            Console.Error.WriteLine("  Error: Failed to parse JSON");
            return 1;
        }

        if (string.IsNullOrEmpty(sampleData.LiquidFile))
        {
            Console.Error.WriteLine("  Error: Missing 'liquid_file' property");
            return 1;
        }

        var timeZone = sampleData.CurrentOrganization?.TimeZone ?? "UTC";
        var renderEngine = new RenderEngine(liquidDir, sampleDataDir, timeZone);
        var inputFileName = Path.GetFileNameWithoutExtension(sampleDataPath);

        // Render the main template (list view or single item view)
        RenderMainTemplate(sampleData, renderEngine, liquidDir, htmlDir, inputFileName);

        // Render individual detail pages for items that have detail_liquid_file
        RenderDetailPages(sampleData, renderEngine, liquidDir, htmlDir);

        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"  Error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.Error.WriteLine($"    Inner: {ex.InnerException.Message}");
        }
        return 1;
    }
}

static void RenderMainTemplate(SampleData sampleData, RenderEngine renderEngine, string liquidDir, string htmlDir, string inputFileName)
{
    var liquidFile = sampleData.LiquidFile;
    if (!liquidFile.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase))
    {
        liquidFile += ".liquid";
    }

    var templatePath = Path.Combine(liquidDir, liquidFile);
    if (!File.Exists(templatePath))
    {
        throw new FileNotFoundException($"Template not found: {templatePath}");
    }

    var templateSource = File.ReadAllText(templatePath);
    var context = SimulatorContext.FromSampleData(sampleData);

    Console.WriteLine($"  Rendering list template...");
    var html = renderEngine.RenderAsHtml(templateSource, context);

    var outputFileName = $"{inputFileName}.html";
    var outputFilePath = Path.Combine(htmlDir, outputFileName);

    File.WriteAllText(outputFilePath, html);
    Console.WriteLine($"    -> {outputFileName}");
}

static void RenderDetailPages(SampleData sampleData, RenderEngine renderEngine, string liquidDir, string htmlDir)
{
    // Get items from Target
    var target = SimulatorContext.ConvertJsonElement(sampleData.Target) as Dictionary<string, object?>;
    if (target == null || !target.TryGetValue("Items", out var itemsObj) || itemsObj is not List<object?> items)
    {
        return;
    }

    // Filter items that have detail_liquid_file
    var itemsWithDetailTemplate = items
        .OfType<Dictionary<string, object?>>()
        .Where(item => item.TryGetValue("detail_liquid_file", out var val) && val is string s && !string.IsNullOrEmpty(s))
        .ToList();

    if (itemsWithDetailTemplate.Count == 0)
    {
        return;
    }

    Console.WriteLine($"  Rendering {itemsWithDetailTemplate.Count} detail page(s)...");

    // Cache loaded templates
    var templateCache = new Dictionary<string, string>();

    foreach (var item in itemsWithDetailTemplate)
    {
        // Get the detail template for this item
        var detailLiquidFile = (string)item["detail_liquid_file"]!;
        if (!detailLiquidFile.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase))
        {
            detailLiquidFile += ".liquid";
        }

        // Load template (with caching)
        if (!templateCache.TryGetValue(detailLiquidFile, out var detailTemplateSource))
        {
            var detailTemplatePath = Path.Combine(liquidDir, detailLiquidFile);
            if (!File.Exists(detailTemplatePath))
            {
                Console.Error.WriteLine($"    Warning: Template not found: {detailLiquidFile}");
                continue;
            }
            detailTemplateSource = File.ReadAllText(detailTemplatePath);
            templateCache[detailLiquidFile] = detailTemplateSource;
        }

        // Get the RoutePath to determine the output filename
        if (!item.TryGetValue("RoutePath", out var routePathObj) || routePathObj is not string routePath)
            continue;

        // The RoutePath should be the HTML filename (e.g., "posts_getting-started.html")
        var outputFileName = routePath;
        if (!outputFileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            outputFileName += ".html";
        }

        // Create a detail context with this item as Target
        var detailContext = new SimulatorContext
        {
            Target = item,
            ContentType = sampleData.ContentType,
            CurrentOrganization = sampleData.CurrentOrganization ?? new OrganizationModel { OrganizationName = "Sample Organization" },
            CurrentUser = sampleData.CurrentUser ?? new UserModel(),
            PathBase = sampleData.PathBase,
            QueryParams = sampleData.QueryParams ?? new Dictionary<string, string>()
        };

        var html = renderEngine.RenderAsHtml(detailTemplateSource, detailContext);

        var outputFilePath = Path.Combine(htmlDir, outputFileName);
        File.WriteAllText(outputFilePath, html);
        Console.WriteLine($"    -> {outputFileName}");
    }
}

static void PrintUsage()
{
    Console.WriteLine("Raytha Template Simulator");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run -- [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --site <name>         Project name (default: 'default')");
    Console.WriteLine("  -o, --output <path>   Custom output directory for HTML");
    Console.WriteLine("  -s, --sync            Sync templates to Raytha after rendering");
    Console.WriteLine("  --sync-only           Only sync templates (skip rendering)");
    Console.WriteLine("  -h, --help            Show this help message");
    Console.WriteLine();
    Console.WriteLine("Project Structure:");
    Console.WriteLine("  Projects live in /dist/<site-name>/ with:");
    Console.WriteLine("    - liquid/           Templates (copied from src/liquid on init)");
    Console.WriteLine("    - widgets/          Widget templates (in liquid/widgets/)");
    Console.WriteLine("    - sample-data/      Your site's JSON data files");
    Console.WriteLine("    - models/           Content type definitions");
    Console.WriteLine("    - htmlOutput/       Generated HTML files");
    Console.WriteLine();
    Console.WriteLine("Getting Started:");
    Console.WriteLine("  1. Run 'dotnet run -- --site mysite' to initialize a new project");
    Console.WriteLine("  2. Add sample data (site-pages.json, menus.json, etc.)");
    Console.WriteLine("  3. Customize templates in dist/mysite/liquid/");
    Console.WriteLine("  4. Run again to render HTML");
    Console.WriteLine();
    Console.WriteLine("Sample Data Files:");
    Console.WriteLine("  - menus.json          Navigation menus");
    Console.WriteLine("  - site-pages.json     Site pages with widgets");
    Console.WriteLine("  - <type>.json         Content type data (e.g., posts.json)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run                        # Render default project");
    Console.WriteLine("  dotnet run -- --site myblog       # Initialize or render 'myblog'");
    Console.WriteLine("  dotnet run -- --site myblog --sync # Render and sync to Raytha");
}

static string FindProjectRoot(string startDir)
{
    var dir = startDir;
    while (dir != null)
    {
        // Look for src directory with our code
        if (Directory.Exists(Path.Combine(dir, "src", "liquid")) ||
            Directory.Exists(Path.Combine(dir, ".git")))
        {
            return dir;
        }
        dir = Directory.GetParent(dir)?.FullName;
    }
    return startDir;
}
