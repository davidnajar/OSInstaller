using System.Text.Json;

namespace OSInstaller.Models;

public class UnifiedSpec
{
    public List<Page> Pages { get; set; } = new();
    public JsonElement OutputTemplate { get; set; }
    public List<string> Diagnostics { get; set; } = new();
}
