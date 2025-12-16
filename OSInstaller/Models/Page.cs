namespace OSInstaller.Models;

public class Page
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Description { get; set; }
    public List<Field> Fields { get; set; } = new();
}
