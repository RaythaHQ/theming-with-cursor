using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using RaythaSimulator.Models;

namespace RaythaSimulator;

/// <summary>
/// Client for syncing templates with the Raytha CMS API.
///
/// IMPORTANT: The {% layout 'name' %} tag is LOCAL-ONLY and is stripped
/// before publishing to Raytha. In Raytha, template inheritance is handled
/// via the parentTemplateDeveloperName field in the API.
///
/// The {% renderbody %} tag works in both local simulator and production Raytha.
/// </summary>
public partial class RaythaApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RaythaConfig _config;
    private readonly string _liquidDirectory;
    private readonly string _widgetsDirectory;

    // Regex patterns for local-only tags that must be stripped before publishing
    [GeneratedRegex(@"\{%\s*layout\s+['""]([^'""]+)['""]\s*%\}", RegexOptions.IgnoreCase)]
    private static partial Regex LayoutTagRegex();

    [GeneratedRegex(@"\{%\s*renderbody\s*%\}", RegexOptions.IgnoreCase)]
    private static partial Regex RenderBodyTagRegex();

    // Pattern to match .html extensions in local URLs (e.g., {{ PathBase }}/{{ something }}.html)
    [GeneratedRegex(@"\}\}\.html(?=[""'\s>])")]
    private static partial Regex LocalHtmlExtensionRegex();

    public RaythaApiClient(RaythaConfig config, string liquidDirectory)
    {
        _config = config;
        _liquidDirectory = liquidDirectory;
        _widgetsDirectory = Path.Combine(liquidDirectory, "widgets");
        _httpClient = new HttpClient { BaseAddress = new Uri(config.BaseUrl.TrimEnd('/')) };
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", config.ApiKey);
    }

    /// <summary>
    /// Discovers all templates from the liquid directory and syncs them to Raytha.
    /// Template developer names are derived from filenames.
    /// Parent relationships are detected from {% layout 'name' %} tags.
    /// Base layouts are detected by presence of {% renderbody %} tag.
    /// </summary>
    public async Task<SyncResult> SyncAllTemplatesAsync()
    {
        var result = new SyncResult();

        // Discover templates from liquid directory
        var templates = DiscoverTemplates();

        if (templates.Count == 0)
        {
            Console.WriteLine("No .liquid files found in liquid directory");
        }
        else
        {
            Console.WriteLine($"\nSyncing web templates to Raytha ({_config.BaseUrl})...");
            Console.WriteLine($"Theme: {_config.ThemeDeveloperName}");
            Console.WriteLine($"Found {templates.Count} web template(s)");
            Console.WriteLine();

            // Sort templates so base layouts are synced first (they have no parent)
            var sortedTemplates = templates
                .OrderBy(t => t.Value.ParentTemplateDeveloperName != null)
                .ThenBy(t => t.Key);

            foreach (var (developerName, templateInfo) in sortedTemplates)
            {
                try
                {
                    await SyncTemplateAsync(developerName, templateInfo);
                    result.Succeeded.Add(developerName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"  ERROR: {ex.Message}");
                    result.Failed.Add((developerName, ex.Message));
                }
            }
        }

        // Sync widget templates
        var widgetResult = await SyncAllWidgetTemplatesAsync();
        result.WidgetSucceeded.AddRange(widgetResult.WidgetSucceeded);
        result.WidgetFailed.AddRange(widgetResult.WidgetFailed);

        Console.WriteLine();
        Console.WriteLine(
            $"Sync complete: {result.Succeeded.Count} web templates, {result.WidgetSucceeded.Count} widgets succeeded"
        );
        if (result.Failed.Count > 0 || result.WidgetFailed.Count > 0)
        {
            Console.WriteLine(
                $"  Failed: {result.Failed.Count} web templates, {result.WidgetFailed.Count} widgets"
            );
        }

        return result;
    }

    /// <summary>
    /// Discovers all widget templates from the widgets directory and syncs them to Raytha.
    /// Widget developer names are derived from filenames.
    /// </summary>
    public async Task<SyncResult> SyncAllWidgetTemplatesAsync()
    {
        var result = new SyncResult();

        // Discover widget templates from widgets directory
        var widgets = DiscoverWidgetTemplates();

        if (widgets.Count == 0)
        {
            Console.WriteLine("\nNo widget templates found in widgets directory");
            return result;
        }

        Console.WriteLine($"\nSyncing widget templates...");
        Console.WriteLine($"Found {widgets.Count} widget template(s)");
        Console.WriteLine();

        foreach (var (developerName, widgetInfo) in widgets.OrderBy(w => w.Key))
        {
            try
            {
                await SyncWidgetTemplateAsync(developerName, widgetInfo);
                result.WidgetSucceeded.Add(developerName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  ERROR: {ex.Message}");
                result.WidgetFailed.Add((developerName, ex.Message));
            }
        }

        return result;
    }

    /// <summary>
    /// Discovers templates from the liquid directory.
    /// - Developer name = filename without .liquid extension
    /// - Parent layout = extracted from {% layout 'name' %} tag
    /// - isBaseLayout = true if template contains {% renderbody %}
    /// - Label = can be overridden in config, defaults to developer name with formatting
    /// </summary>
    private Dictionary<string, DiscoveredTemplate> DiscoverTemplates()
    {
        var templates = new Dictionary<string, DiscoveredTemplate>();

        if (!Directory.Exists(_liquidDirectory))
            return templates;

        foreach (var filePath in Directory.GetFiles(_liquidDirectory, "*.liquid"))
        {
            var developerName = Path.GetFileNameWithoutExtension(filePath);
            var content = File.ReadAllText(filePath);

            // Extract parent layout from {% layout 'name' %} tag
            string? parentLayout = null;
            var layoutMatch = LayoutTagRegex().Match(content);
            if (layoutMatch.Success)
            {
                parentLayout = layoutMatch.Groups[1].Value;
            }

            // Check if this is a base layout (contains {% renderbody %})
            var isBaseLayout = RenderBodyTagRegex().IsMatch(content);

            // Check for config overrides
            var configOverride = _config.Templates?.GetValueOrDefault(developerName);

            templates[developerName] = new DiscoveredTemplate
            {
                FilePath = filePath,
                Content = content,
                Label = !string.IsNullOrEmpty(configOverride?.Label)
                    ? configOverride.Label
                    : FormatLabel(developerName),
                IsBaseLayout = isBaseLayout,
                ParentTemplateDeveloperName = parentLayout,
                AllowAccessForNewContentTypes =
                    configOverride?.AllowAccessForNewContentTypes ?? true,
                TemplateAccessToModelDefinitions = configOverride?.TemplateAccessToModelDefinitions,
            };
        }

        return templates;
    }

    /// <summary>
    /// Discovers widget templates from the widgets directory.
    /// - Developer name = filename without .liquid extension
    /// - Label = can be overridden in config, defaults to developer name with formatting
    /// </summary>
    private Dictionary<string, DiscoveredWidget> DiscoverWidgetTemplates()
    {
        var widgets = new Dictionary<string, DiscoveredWidget>();

        if (!Directory.Exists(_widgetsDirectory))
            return widgets;

        foreach (var filePath in Directory.GetFiles(_widgetsDirectory, "*.liquid"))
        {
            var developerName = Path.GetFileNameWithoutExtension(filePath);
            var content = File.ReadAllText(filePath);

            // Check for config overrides
            var configOverride = _config.Widgets?.GetValueOrDefault(developerName);

            widgets[developerName] = new DiscoveredWidget
            {
                FilePath = filePath,
                Content = content,
                Label = !string.IsNullOrEmpty(configOverride?.Label)
                    ? configOverride.Label
                    : FormatLabel(developerName),
            };
        }

        return widgets;
    }

    /// <summary>
    /// Formats a developer name into a human-readable label.
    /// e.g., "raytha_html_base_layout" -> "Raytha Html Base Layout"
    /// </summary>
    private static string FormatLabel(string developerName)
    {
        // Replace underscores with spaces and title-case each word
        var words = developerName.Split('_');
        var formatted = words.Select(w =>
            string.IsNullOrEmpty(w) ? w : char.ToUpper(w[0]) + (w.Length > 1 ? w[1..] : "")
        );
        return string.Join(" ", formatted);
    }

    /// <summary>
    /// Syncs a single template to Raytha.
    /// Uses "update-first" pattern: tries to update, falls back to create if 404.
    /// This is more robust than "check then act" which can have race conditions.
    /// </summary>
    public async Task SyncTemplateAsync(string developerName, DiscoveredTemplate template)
    {
        Console.Write($"  {developerName}");
        if (template.IsBaseLayout)
            Console.Write(" [layout]");
        if (template.ParentTemplateDeveloperName != null)
            Console.Write($" -> {template.ParentTemplateDeveloperName}");
        Console.Write("... ");

        // Strip local-only tags before publishing
        var cleanedContent = StripLocalOnlyTags(template.Content);

        // Try update first, fall back to create if template doesn't exist
        var updated = await TryUpdateTemplateAsync(developerName, template, cleanedContent);

        if (updated)
        {
            Console.WriteLine("updated");
        }
        else
        {
            await CreateTemplateAsync(developerName, template, cleanedContent);
            Console.WriteLine("created");
        }
    }

    /// <summary>
    /// Syncs a single widget template to Raytha.
    /// Widget templates are update-only since they are created by Raytha with the theme.
    /// </summary>
    public async Task SyncWidgetTemplateAsync(string developerName, DiscoveredWidget widget)
    {
        Console.Write($"  {developerName}... ");

        // Strip local-only tags before publishing
        var cleanedContent = StripLocalOnlyTags(widget.Content);

        var updated = await TryUpdateWidgetTemplateAsync(developerName, widget, cleanedContent);

        if (updated)
        {
            Console.WriteLine("updated");
        }
        else
        {
            Console.WriteLine("not found (widget must exist in Raytha)");
        }
    }

    /// <summary>
    /// Tries to update an existing widget template in Raytha.
    /// Returns true if updated successfully, false if widget doesn't exist (404).
    /// Throws on other errors.
    /// </summary>
    private async Task<bool> TryUpdateWidgetTemplateAsync(
        string developerName,
        DiscoveredWidget widget,
        string content
    )
    {
        var requestBody = new UpdateWidgetTemplateRequest
        {
            Label = widget.Label,
            Content = content,
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/raytha/api/v1/WidgetTemplates/theme/{_config.ThemeDeveloperName}/template/{developerName}",
            requestBody,
            JsonOptions
        );

        // If 404, widget doesn't exist - return false
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, "update widget template");
        return true;
    }

    /// <summary>
    /// Strips local-only content from templates before publishing to Raytha.
    ///
    /// Local-only content:
    /// - {% layout 'name' %} tag - Used locally to specify parent layout
    /// - .html extensions in URLs - Local simulator uses .html files, Raytha doesn't
    ///
    /// In Raytha, layout inheritance is handled via the parentTemplateDeveloperName
    /// field in the API. The {% renderbody %} tag works in both local and production.
    /// </summary>
    public static string StripLocalOnlyTags(string content)
    {
        // Remove {% layout 'name' %} tags (local-only)
        var result = LayoutTagRegex().Replace(content, string.Empty);

        // Remove .html from local URLs (e.g., {{ PathBase }}/{{ Type }}.html" -> {{ PathBase }}/{{ Type }}")
        result = LocalHtmlExtensionRegex().Replace(result, "}}");

        // Clean up any leading/trailing whitespace but preserve internal structure
        return result.Trim();
    }

    /// <summary>
    /// Creates a new template in Raytha.
    /// </summary>
    private async Task CreateTemplateAsync(
        string developerName,
        DiscoveredTemplate template,
        string content
    )
    {
        var requestBody = new CreateTemplateRequest
        {
            DeveloperName = developerName,
            Label = template.Label,
            Content = content,
            IsBaseLayout = template.IsBaseLayout,
            ParentTemplateDeveloperName = template.ParentTemplateDeveloperName,
            AllowAccessForNewContentTypes = template.AllowAccessForNewContentTypes,
            TemplateAccessToModelDefinitions = template.TemplateAccessToModelDefinitions,
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/raytha/api/v1/WebTemplates/theme/{_config.ThemeDeveloperName}",
            requestBody,
            JsonOptions
        );

        await EnsureSuccessAsync(response, "create template");
    }

    /// <summary>
    /// Tries to update an existing template in Raytha.
    /// Returns true if updated successfully, false if template doesn't exist (404).
    /// Throws on other errors.
    /// </summary>
    private async Task<bool> TryUpdateTemplateAsync(
        string developerName,
        DiscoveredTemplate template,
        string content
    )
    {
        var requestBody = new UpdateTemplateRequest
        {
            DeveloperName = developerName,
            Label = template.Label,
            Content = content,
            IsBaseLayout = template.IsBaseLayout,
            ParentTemplateDeveloperName = template.ParentTemplateDeveloperName,
            AllowAccessForNewContentTypes = template.AllowAccessForNewContentTypes,
            TemplateAccessToModelDefinitions = template.TemplateAccessToModelDefinitions,
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/raytha/api/v1/WebTemplates/theme/{_config.ThemeDeveloperName}/template/{developerName}",
            requestBody,
            JsonOptions
        );

        // If 404, template doesn't exist - return false to trigger create
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, "update template");
        return true;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Failed to {operation}: {response.StatusCode} - {errorContent}"
            );
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

/// <summary>
/// Result of a template sync operation
/// </summary>
public class SyncResult
{
    public List<string> Succeeded { get; } = new();
    public List<(string TemplateName, string Error)> Failed { get; } = new();
    public List<string> WidgetSucceeded { get; } = new();
    public List<(string WidgetName, string Error)> WidgetFailed { get; } = new();
}

#region API Request/Response Models

internal class CreateTemplateRequest
{
    [JsonPropertyName("developerName")]
    public string DeveloperName { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("isBaseLayout")]
    public bool IsBaseLayout { get; set; }

    [JsonPropertyName("parentTemplateDeveloperName")]
    public string? ParentTemplateDeveloperName { get; set; }

    [JsonPropertyName("allowAccessForNewContentTypes")]
    public bool AllowAccessForNewContentTypes { get; set; } = true;

    [JsonPropertyName("templateAccessToModelDefinitions")]
    public List<string>? TemplateAccessToModelDefinitions { get; set; }
}

internal class UpdateTemplateRequest
{
    [JsonPropertyName("developerName")]
    public string DeveloperName { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("isBaseLayout")]
    public bool IsBaseLayout { get; set; }

    [JsonPropertyName("parentTemplateDeveloperName")]
    public string? ParentTemplateDeveloperName { get; set; }

    [JsonPropertyName("allowAccessForNewContentTypes")]
    public bool AllowAccessForNewContentTypes { get; set; } = true;

    [JsonPropertyName("templateAccessToModelDefinitions")]
    public List<string>? TemplateAccessToModelDefinitions { get; set; }
}

internal class UpdateWidgetTemplateRequest
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Represents a template discovered from the liquid directory
/// </summary>
public class DiscoveredTemplate
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsBaseLayout { get; set; }
    public string? ParentTemplateDeveloperName { get; set; }
    public bool AllowAccessForNewContentTypes { get; set; } = true;
    public List<string>? TemplateAccessToModelDefinitions { get; set; }
}

/// <summary>
/// Represents a widget template discovered from the widgets directory
/// </summary>
public class DiscoveredWidget
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
