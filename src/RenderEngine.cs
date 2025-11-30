using System.Text.Json;
using System.Text.RegularExpressions;
using Fluid;
using Fluid.Values;
using RaythaSimulator.Models;

namespace RaythaSimulator;

/// <summary>
/// Raytha template simulator render engine using Fluid
/// </summary>
public partial class RenderEngine
{
    private static readonly FluidParser _parser = new(new FluidParserOptions { AllowFunctions = true });
    private readonly string _liquidDirectory;
    private readonly string _sampleDataDirectory;
    private readonly TimeZoneInfo _timeZone;

    // Regex to extract {% layout 'name' %} from template
    [GeneratedRegex(@"\{%\s*layout\s+['""]([^'""]+)['""]\s*%\}", RegexOptions.IgnoreCase)]
    private static partial Regex LayoutTagRegex();

    public RenderEngine(string liquidDirectory, string sampleDataDirectory, string timeZone = "UTC")
    {
        _liquidDirectory = liquidDirectory;
        _sampleDataDirectory = sampleDataDirectory;
        _timeZone = GetTimeZoneInfo(timeZone);
    }

    /// <summary>
    /// Renders a template with the given context
    /// </summary>
    public string RenderAsHtml(string source, SimulatorContext context)
    {
        // Check for layout tag and extract it
        var layoutMatch = LayoutTagRegex().Match(source);
        string? layoutName = null;

        if (layoutMatch.Success)
        {
            layoutName = layoutMatch.Groups[1].Value;
            // Remove the layout tag from the source
            source = LayoutTagRegex().Replace(source, string.Empty).TrimStart();
        }

        // Parse and render the child template first
        if (!_parser.TryParse(source, out var template, out var error))
        {
            throw new InvalidOperationException($"Template parsing error: {error}");
        }

        var options = CreateTemplateOptions();
        var fluidContext = new TemplateContext(options);

        // Set all the context values
        SetContextValues(fluidContext, context);

        string renderedHtml = template.Render(fluidContext);

        // If there's a layout, render it and inject the child content
        if (!string.IsNullOrEmpty(layoutName))
        {
            renderedHtml = RenderWithLayout(renderedHtml, layoutName, context);
        }

        return renderedHtml;
    }

    /// <summary>
    /// Renders the child content within a parent layout
    /// </summary>
    private string RenderWithLayout(string childContent, string layoutName, SimulatorContext context)
    {
        // Load the layout template
        var layoutPath = Path.Combine(_liquidDirectory, $"{layoutName}.liquid");
        if (!File.Exists(layoutPath))
        {
            throw new FileNotFoundException($"Layout template not found: {layoutPath}");
        }

        var layoutSource = File.ReadAllText(layoutPath);

        // Check if layout also has a parent (recursive)
        var layoutMatch = LayoutTagRegex().Match(layoutSource);
        string? parentLayoutName = null;

        if (layoutMatch.Success)
        {
            parentLayoutName = layoutMatch.Groups[1].Value;
            layoutSource = LayoutTagRegex().Replace(layoutSource, string.Empty).TrimStart();
        }

        // Replace {% renderbody %} with the child content
        layoutSource = Regex.Replace(layoutSource, @"\{%\s*renderbody\s*%\}", childContent, RegexOptions.IgnoreCase);

        // Parse and render the layout
        if (!_parser.TryParse(layoutSource, out var layoutTemplate, out var error))
        {
            throw new InvalidOperationException($"Layout template parsing error: {error}");
        }

        var options = CreateTemplateOptions();
        var fluidContext = new TemplateContext(options);
        SetContextValues(fluidContext, context);

        string renderedHtml = layoutTemplate.Render(fluidContext);

        // Recursively apply parent layout if exists
        if (!string.IsNullOrEmpty(parentLayoutName))
        {
            renderedHtml = RenderWithLayout(renderedHtml, parentLayoutName, context);
        }

        return renderedHtml;
    }

    private TemplateOptions CreateTemplateOptions()
    {
        var options = new TemplateOptions();
        options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
        options.TimeZone = _timeZone;

        // Register custom filters
        options.Filters.AddFilter("attachment_redirect_url", AttachmentRedirectUrl);
        options.Filters.AddFilter("attachment_public_url", AttachmentPublicUrl);
        options.Filters.AddFilter("organization_time", OrganizationTimeFilter);
        options.Filters.AddFilter("groupby", GroupBy);
        options.Filters.AddFilter("json", JsonFilter);

        return options;
    }

    private void SetContextValues(TemplateContext fluidContext, SimulatorContext context)
    {
        // Set main context values
        fluidContext.SetValue("Target", context.Target);
        fluidContext.SetValue("ContentType", context.ContentType);
        fluidContext.SetValue("CurrentOrganization", context.CurrentOrganization);
        fluidContext.SetValue("CurrentUser", context.CurrentUser);
        fluidContext.SetValue("PathBase", context.PathBase);
        fluidContext.SetValue("QueryParams", context.QueryParams);

        // Set custom functions
        fluidContext.SetValue("get_content_item_by_id", GetContentItemById());
        fluidContext.SetValue("get_content_items", GetContentItems());
        fluidContext.SetValue("get_content_type_by_developer_name", GetContentTypeByDeveloperName());
        fluidContext.SetValue("get_main_menu", GetMainMenu());
        fluidContext.SetValue("get_menu", GetMenuByDeveloperName());
        fluidContext.SetValue("render_section", RenderSection());
    }

    #region Custom Filters

    // Common image extensions
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ico"
    };

    private static ValueTask<FluidValue> AttachmentRedirectUrl(
        FluidValue input,
        FilterArguments arguments,
        TemplateContext context)
    {
        var value = input.ToStringValue();
        if (string.IsNullOrEmpty(value))
            return new StringValue(string.Empty);

        // Return placeholder image for simulator
        return new StringValue(GetPlaceholderUrl(value));
    }

    private static ValueTask<FluidValue> AttachmentPublicUrl(
        FluidValue input,
        FilterArguments arguments,
        TemplateContext context)
    {
        var value = input.ToStringValue();
        if (string.IsNullOrEmpty(value))
            return new StringValue(string.Empty);

        // Return placeholder image for simulator
        return new StringValue(GetPlaceholderUrl(value));
    }

    private static string GetPlaceholderUrl(string filename)
    {
        var extension = Path.GetExtension(filename);
        
        // For images, return a placeholder image
        if (ImageExtensions.Contains(extension))
        {
            // Use placehold.co for placeholder images
            return "https://placehold.co/400x300/e2e8f0/64748b?text=Image+Placeholder";
        }
        
        // For other file types, return a generic file placeholder
        return "https://placehold.co/200x200/f1f5f9/475569?text=File";
    }

    private ValueTask<FluidValue> OrganizationTimeFilter(
        FluidValue input,
        FilterArguments arguments,
        TemplateContext context)
    {
        if (!input.TryGetDateTimeInput(context, out var value))
        {
            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        var utc = DateTime.SpecifyKind(value.DateTime, DateTimeKind.Utc);
        var localOffset = new DateTimeOffset(utc, TimeSpan.Zero);
        var result = TimeZoneInfo.ConvertTime(localOffset, _timeZone);

        // Apply date format if provided
        var format = arguments.At(0).ToStringValue();
        if (!string.IsNullOrEmpty(format))
        {
            // Convert Liquid date format to .NET format
            var formatted = FormatDateTime(result.DateTime, format);
            return new ValueTask<FluidValue>(new StringValue(formatted));
        }

        return new ValueTask<FluidValue>(new DateTimeValue(result));
    }

    private static string FormatDateTime(DateTime dateTime, string format)
    {
        // Convert common Liquid date formats to .NET formats
        var netFormat = format
            .Replace("%b", "MMM")
            .Replace("%B", "MMMM")
            .Replace("%d", "dd")
            .Replace("%e", "d")
            .Replace("%Y", "yyyy")
            .Replace("%y", "yy")
            .Replace("%H", "HH")
            .Replace("%I", "hh")
            .Replace("%l", "h")
            .Replace("%M", "mm")
            .Replace("%S", "ss")
            .Replace("%p", "tt")
            .Replace("%P", "tt")
            .Replace("%c", "F")
            .Replace("%a", "ddd")
            .Replace("%A", "dddd");

        return dateTime.ToString(netFormat);
    }

    private static ValueTask<FluidValue> GroupBy(
        FluidValue input,
        FilterArguments arguments,
        TemplateContext context)
    {
        var groupByProperty = arguments.At(0).ToStringValue();
        var groups = input
            .Enumerate(context)
            .GroupBy(p => GetPropertyValue(p, groupByProperty, context));

        var result = new List<FluidValue>();
        foreach (var group in groups)
        {
            result.Add(new ObjectValue(new { key = group.Key, items = group.ToList() }));
        }

        return new ValueTask<FluidValue>(new ArrayValue(result));
    }

    private static string GetPropertyValue(FluidValue p, string propertyPath, TemplateContext context)
    {
        var parts = propertyPath.Split('.');
        FluidValue current = p;

        foreach (var part in parts)
        {
            current = current.GetValueAsync(part, context).Result;
        }

        return current.ToStringValue();
    }

    private static ValueTask<FluidValue> JsonFilter(
        FluidValue input,
        FilterArguments arguments,
        TemplateContext context)
    {
        var obj = input.ToObjectValue();
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        return new ValueTask<FluidValue>(new StringValue(json));
    }

    #endregion

    #region Custom Functions

    private FunctionValue GetContentItemById()
    {
        return new FunctionValue((args, context) =>
        {
            var contentItemId = args.At(0).ToStringValue();
            // In simulator, try to find in any loaded sample data
            // For now, return nil
            return new ValueTask<FluidValue>(NilValue.Instance);
        });
    }

    private FunctionValue GetContentItems()
    {
        return new FunctionValue((args, context) =>
        {
            var contentType = args["ContentType"].ToStringValue();
            var filter = args["Filter"].ToStringValue();
            var orderBy = args["OrderBy"].ToStringValue();
            var pageNumber = (int)args["PageNumber"].ToNumberValue();
            var pageSize = (int)args["PageSize"].ToNumberValue();

            if (pageSize <= 0) pageSize = 25;
            if (pageNumber <= 0) pageNumber = 1;

            // Try to load from sample data file
            var sampleFile = Path.Combine(_sampleDataDirectory, $"{contentType}.json");
            if (File.Exists(sampleFile))
            {
                try
                {
                    var json = File.ReadAllText(sampleFile);
                    var sampleData = JsonSerializer.Deserialize<SampleData>(json);
                    if (sampleData?.Target.ValueKind == JsonValueKind.Object)
                    {
                        var target = SimulatorContext.ConvertJsonElement(sampleData.Target) as Dictionary<string, object?>;
                        if (target != null && target.TryGetValue("Items", out var items) && items is List<object?> itemsList)
                        {
                            var result = new Dictionary<string, object?>
                            {
                                ["Items"] = itemsList.Take(pageSize).ToList(),
                                ["TotalCount"] = itemsList.Count
                            };
                            return new ValueTask<FluidValue>(new ObjectValue(result));
                        }
                    }
                }
                catch
                {
                    // Ignore errors, return empty result
                }
            }

            // Return empty result
            var emptyResult = new Dictionary<string, object?>
            {
                ["Items"] = new List<object?>(),
                ["TotalCount"] = 0
            };
            return new ValueTask<FluidValue>(new ObjectValue(emptyResult));
        });
    }

    private FunctionValue GetContentTypeByDeveloperName()
    {
        return new FunctionValue((args, context) =>
        {
            var developerName = args.At(0).ToStringValue();

            // Try to load from sample data file
            var sampleFile = Path.Combine(_sampleDataDirectory, $"{developerName}.json");
            if (File.Exists(sampleFile))
            {
                try
                {
                    var json = File.ReadAllText(sampleFile);
                    var sampleData = JsonSerializer.Deserialize<SampleData>(json);
                    if (sampleData?.ContentType != null)
                    {
                        return new ValueTask<FluidValue>(new ObjectValue(sampleData.ContentType));
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        });
    }

    private FunctionValue GetMainMenu()
    {
        return new FunctionValue((_, _) =>
        {
            var menusFile = Path.Combine(_sampleDataDirectory, "menus.json");
            if (File.Exists(menusFile))
            {
                try
                {
                    var json = File.ReadAllText(menusFile);
                    var menusData = JsonSerializer.Deserialize<MenusData>(json);
                    var mainMenu = menusData?.Menus?.FirstOrDefault(m => m.IsMainMenu);
                    if (mainMenu != null)
                    {
                        return new ValueTask<FluidValue>(new ObjectValue(mainMenu));
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            // Return empty menu
            return new ValueTask<FluidValue>(new ObjectValue(new NavigationMenuModel { MenuItems = new List<NavigationMenuItemModel>() }));
        });
    }

    private FunctionValue GetMenuByDeveloperName()
    {
        return new FunctionValue((args, _) =>
        {
            var developerName = args.At(0).ToStringValue();
            var menusFile = Path.Combine(_sampleDataDirectory, "menus.json");

            if (File.Exists(menusFile))
            {
                try
                {
                    var json = File.ReadAllText(menusFile);
                    var menusData = JsonSerializer.Deserialize<MenusData>(json);
                    var menu = menusData?.Menus?.FirstOrDefault(m =>
                        m.DeveloperName.Equals(developerName, StringComparison.OrdinalIgnoreCase));
                    if (menu != null)
                    {
                        return new ValueTask<FluidValue>(new ObjectValue(menu));
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            // Return empty menu
            return new ValueTask<FluidValue>(new ObjectValue(new NavigationMenuModel { MenuItems = new List<NavigationMenuItemModel>() }));
        });
    }

    private FunctionValue RenderSection()
    {
        return new FunctionValue((args, _) =>
        {
            var sectionName = args.At(0).ToStringValue();
            // Return a placeholder for the simulator
            var placeholder = $"""
                <div class="simulator-section-placeholder" style="background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); border: 2px dashed #6c757d; border-radius: 8px; padding: 2rem; text-align: center; margin: 1rem 0;">
                    <div style="color: #495057; font-size: 0.875rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.5rem;">Section</div>
                    <div style="color: #212529; font-weight: 600; font-size: 1.25rem;">{sectionName}</div>
                    <div style="color: #6c757d; font-size: 0.75rem; margin-top: 0.5rem;">Widgets render here in Raytha</div>
                </div>
                """;
            return new ValueTask<FluidValue>(new StringValue(placeholder));
        });
    }

    #endregion

    private static TimeZoneInfo GetTimeZoneInfo(string timeZone)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}

