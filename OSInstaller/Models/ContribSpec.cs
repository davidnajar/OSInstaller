using System.Text.Json;

namespace OSInstaller.Models;

public class ContribSpec
{
    public string ContribId { get; set; } = string.Empty;
    public int Priority { get; set; } = 100;
    public List<Page> Pages { get; set; } = new();
    public List<PagePatch> PagePatches { get; set; } = new();
    public JsonElement? OutputTemplatePatch { get; set; }
}
