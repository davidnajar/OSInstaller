using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using OSInstaller.Models;
using OSInstaller.Services;

namespace OSInstaller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WizardController : ControllerBase
{
    private readonly WizardStateService _stateService;
    private readonly ILogger<WizardController> _logger;
    private readonly IWebHostEnvironment _environment;

    public WizardController(
        WizardStateService stateService,
        ILogger<WizardController> logger,
        IWebHostEnvironment environment)
    {
        _stateService = stateService;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("state")]
    public async Task<ActionResult<WizardState>> GetState()
    {
        try
        {
            var state = await _stateService.LoadStateAsync();
            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get wizard state");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("state")]
    public async Task<ActionResult> SaveState([FromBody] WizardState state)
    {
        try
        {
            await _stateService.SaveStateAsync(state);
            return Ok(new { message = "State saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save wizard state");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("state")]
    public async Task<ActionResult> ClearState()
    {
        try
        {
            await _stateService.ClearStateAsync();
            return Ok(new { message = "State cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear wizard state");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<JsonElement>> GenerateOutput([FromBody] GenerateOutputRequest request)
    {
        try
        {
            var outputDoc = JsonDocument.Parse(JsonSerializer.Serialize(request.OutputTemplate));
            var output = outputDoc.RootElement.Clone();

            // Apply values using JSONPath
            foreach (var kvp in request.Values)
            {
                var fieldId = kvp.Key;
                var value = kvp.Value;

                // Find the field to get its JSONPath
                var field = FindFieldInPages(request.Pages, fieldId);
                if (field == null || string.IsNullOrEmpty(field.JsonPath))
                {
                    _logger.LogWarning("Field {FieldId} not found or has no JSONPath", fieldId);
                    continue;
                }

                // Apply the value using JSONPath
                output = ApplyJsonPathValue(output, field.JsonPath, value);
            }

            // Save the final output
            var outputPath = _environment.IsDevelopment() 
                ? Path.Combine(_environment.ContentRootPath, "output.json")
                : "/var/lib/installer/output.json";
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await System.IO.File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(output, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            _logger.LogInformation("Generated output JSON at {Path}", outputPath);

            return Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate output");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private Field? FindFieldInPages(List<Page> pages, string fieldId)
    {
        foreach (var page in pages)
        {
            var field = page.Fields.FirstOrDefault(f => f.Id == fieldId);
            if (field != null)
            {
                return field;
            }
        }
        return null;
    }

    private JsonElement ApplyJsonPathValue(JsonElement root, string jsonPath, JsonElement value)
    {
        try
        {
            // Simple JSONPath implementation for basic cases like $.network.hostname
            // This handles the most common case: $.path.to.property
            if (!jsonPath.StartsWith("$."))
            {
                _logger.LogWarning("JSONPath must start with '$.' : {Path}", jsonPath);
                return root;
            }

            var path = jsonPath.Substring(2); // Remove "$."
            var parts = path.Split('.');

            // Convert root to dictionary for manipulation
            var rootDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(root.GetRawText()) 
                ?? new Dictionary<string, object?>();

            // Navigate to the parent and set the value
            SetNestedValue(rootDict, parts, value);

            // Convert back to JsonElement
            return JsonSerializer.SerializeToElement(rootDict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply JSONPath {Path}", jsonPath);
            return root;
        }
    }

    private void SetNestedValue(Dictionary<string, object?> dict, string[] path, JsonElement value)
    {
        for (int i = 0; i < path.Length - 1; i++)
        {
            if (!dict.ContainsKey(path[i]))
            {
                dict[path[i]] = new Dictionary<string, object?>();
            }

            if (dict[path[i]] is not Dictionary<string, object?> nextDict)
            {
                nextDict = new Dictionary<string, object?>();
                dict[path[i]] = nextDict;
            }

            dict = nextDict;
        }

        // Set the final value
        var lastKey = path[^1];
        dict[lastKey] = ConvertJsonElement(value);
    }

    private object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => JsonSerializer.Deserialize<object>(element.GetRawText())
        };
    }
}

public class GenerateOutputRequest
{
    public Dictionary<string, JsonElement> Values { get; set; } = new();
    public List<Page> Pages { get; set; } = new();
    public JsonElement OutputTemplate { get; set; }
}
