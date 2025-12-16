namespace OSInstaller.Models;

public class PagePatch
{
    public string PageId { get; set; } = string.Empty;
    public string? InsertFieldsAfter { get; set; }
    public string? InsertFieldsBefore { get; set; }
    public List<Field> Fields { get; set; } = new();
}
