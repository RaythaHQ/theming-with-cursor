using System.Text.Json.Serialization;

namespace RaythaSimulator.Models;

/// <summary>
/// Configuration for syncing templates with a Raytha CMS instance.
/// </summary>
public class RaythaConfig
{
    /// <summary>
    /// Base URL of the Raytha instance (e.g., http://localhost:5000)
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>
    /// API key for authenticating with the Raytha API
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Developer name of the theme in Raytha (must already exist)
    /// </summary>
    [JsonPropertyName("themeDeveloperName")]
    public string ThemeDeveloperName { get; set; } = string.Empty;

    /// <summary>
    /// When true, automatically sync templates to Raytha after each render.
    /// When false, work locally only.
    /// </summary>
    [JsonPropertyName("autoSync")]
    public bool AutoSync { get; set; } = false;

    /// <summary>
    /// Optional: Override settings for specific templates.
    /// Templates are auto-discovered from the liquid/ directory.
    /// Key = template developer name (filename without .liquid extension)
    /// </summary>
    [JsonPropertyName("templates")]
    public Dictionary<string, TemplateConfig>? Templates { get; set; }

    /// <summary>
    /// Optional: Override settings for specific widget templates.
    /// Widgets are auto-discovered from the liquid/widgets/ directory.
    /// Key = widget developer name (filename without .liquid extension)
    /// </summary>
    [JsonPropertyName("widgets")]
    public Dictionary<string, WidgetConfig>? Widgets { get; set; }
}

/// <summary>
/// Optional configuration overrides for a template.
/// Most settings are auto-detected from the template file itself.
/// </summary>
public class TemplateConfig
{
    /// <summary>
    /// Human-readable label for the template in Raytha.
    /// If not specified, auto-generated from filename.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Whether new content types should have access to this template
    /// </summary>
    [JsonPropertyName("allowAccessForNewContentTypes")]
    public bool AllowAccessForNewContentTypes { get; set; } = true;

    /// <summary>
    /// List of content type developer names that can use this template
    /// </summary>
    [JsonPropertyName("templateAccessToModelDefinitions")]
    public List<string>? TemplateAccessToModelDefinitions { get; set; }
}

/// <summary>
/// Optional configuration overrides for a widget template.
/// </summary>
public class WidgetConfig
{
    /// <summary>
    /// Human-readable label for the widget in Raytha.
    /// If not specified, auto-generated from filename.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

