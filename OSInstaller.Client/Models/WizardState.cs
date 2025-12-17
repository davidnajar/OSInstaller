using System.Text.Json;

namespace OSInstaller.Client.Models;

public class WizardState
{
    public Dictionary<string, JsonElement> Values { get; set; } = new();
    public Dictionary<string, bool> PageFeatureEnabled { get; set; } = new(); // pageId -> enabled
    public int CurrentPage { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
