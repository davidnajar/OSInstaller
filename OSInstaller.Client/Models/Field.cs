namespace OSInstaller.Client.Models;

public class Field
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public bool Required { get; set; }
    public string JsonPath { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public string? Pattern { get; set; }
    public string? Description { get; set; }
}
