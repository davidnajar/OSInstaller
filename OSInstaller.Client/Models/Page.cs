namespace OSInstaller.Client.Models;

public class FeatureToggle
{
    public string Label { get; set; } = string.Empty;
    public bool Default { get; set; }
}

public class Page
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Description { get; set; }
    public FeatureToggle? FeatureToggle { get; set; }
    public List<Field> Fields { get; set; } = new();
}
