using System.Text.Json;
using OSInstaller.Models;

namespace OSInstaller.Services;

public class SpecMergerService
{
    private readonly ILogger<SpecMergerService> _logger;

    public SpecMergerService(ILogger<SpecMergerService> logger)
    {
        _logger = logger;
    }

    public UnifiedSpec MergeSpecs(List<ContribSpec> specs)
    {
        var unified = new UnifiedSpec();
        var pages = new Dictionary<string, Page>();
        var fieldIds = new HashSet<string>();
        var outputTemplate = new Dictionary<string, object?>();

        foreach (var spec in specs)
        {
            // Add new pages
            foreach (var page in spec.Pages)
            {
                if (pages.ContainsKey(page.Id))
                {
                    unified.Diagnostics.Add($"Error: Page '{page.Id}' is redefined by contrib '{spec.ContribId}'. Use pagePatches instead.");
                    throw new InvalidOperationException($"Page '{page.Id}' is redefined by contrib '{spec.ContribId}'. Use pagePatches instead.");
                }

                // Validate field IDs are unique
                foreach (var field in page.Fields)
                {
                    if (fieldIds.Contains(field.Id))
                    {
                        unified.Diagnostics.Add($"Error: Field ID '{field.Id}' is already defined.");
                        throw new InvalidOperationException($"Field ID '{field.Id}' is already defined.");
                    }
                    fieldIds.Add(field.Id);
                }

                pages[page.Id] = page;
                unified.Diagnostics.Add($"Added page '{page.Id}' from contrib '{spec.ContribId}'");
            }

            // Apply page patches
            foreach (var patch in spec.PagePatches)
            {
                if (!pages.ContainsKey(patch.PageId))
                {
                    unified.Diagnostics.Add($"Warning: PagePatch targets non-existent page '{patch.PageId}' in contrib '{spec.ContribId}'");
                    continue;
                }

                var targetPage = pages[patch.PageId];

                // Validate field IDs are unique
                foreach (var field in patch.Fields)
                {
                    if (fieldIds.Contains(field.Id))
                    {
                        unified.Diagnostics.Add($"Error: Field ID '{field.Id}' is already defined.");
                        throw new InvalidOperationException($"Field ID '{field.Id}' is already defined.");
                    }
                    fieldIds.Add(field.Id);
                }

                // Insert fields
                if (!string.IsNullOrEmpty(patch.InsertFieldsAfter))
                {
                    var index = targetPage.Fields.FindIndex(f => f.Id == patch.InsertFieldsAfter);
                    if (index >= 0)
                    {
                        targetPage.Fields.InsertRange(index + 1, patch.Fields);
                        unified.Diagnostics.Add($"Inserted {patch.Fields.Count} field(s) after '{patch.InsertFieldsAfter}' in page '{patch.PageId}'");
                    }
                    else
                    {
                        unified.Diagnostics.Add($"Warning: Field '{patch.InsertFieldsAfter}' not found in page '{patch.PageId}'");
                    }
                }
                else if (!string.IsNullOrEmpty(patch.InsertFieldsBefore))
                {
                    var index = targetPage.Fields.FindIndex(f => f.Id == patch.InsertFieldsBefore);
                    if (index >= 0)
                    {
                        targetPage.Fields.InsertRange(index, patch.Fields);
                        unified.Diagnostics.Add($"Inserted {patch.Fields.Count} field(s) before '{patch.InsertFieldsBefore}' in page '{patch.PageId}'");
                    }
                    else
                    {
                        unified.Diagnostics.Add($"Warning: Field '{patch.InsertFieldsBefore}' not found in page '{patch.PageId}'");
                    }
                }
                else
                {
                    // Append to end
                    targetPage.Fields.AddRange(patch.Fields);
                    unified.Diagnostics.Add($"Appended {patch.Fields.Count} field(s) to page '{patch.PageId}'");
                }
            }

            // Merge output template patches
            if (spec.OutputTemplatePatch.HasValue)
            {
                MergeJsonElement(outputTemplate, spec.OutputTemplatePatch.Value);
                unified.Diagnostics.Add($"Merged output template from contrib '{spec.ContribId}'");
            }
        }

        unified.Pages = pages.Values.OrderBy(p => p.Order).ToList();
        unified.OutputTemplate = JsonSerializer.SerializeToElement(outputTemplate);

        _logger.LogInformation("Merged {SpecCount} specs into {PageCount} pages with {FieldCount} total fields",
            specs.Count, unified.Pages.Count, fieldIds.Count);

        return unified;
    }

    private void MergeJsonElement(Dictionary<string, object?> target, JsonElement source)
    {
        if (source.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in source.EnumerateObject())
            {
                if (target.ContainsKey(property.Name) && target[property.Name] is Dictionary<string, object?> existingDict)
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        MergeJsonElement(existingDict, property.Value);
                    }
                    else
                    {
                        target[property.Name] = ConvertJsonElement(property.Value);
                    }
                }
                else
                {
                    target[property.Name] = ConvertJsonElement(property.Value);
                }
            }
        }
    }

    private object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<List<object?>>(element.GetRawText()),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }
}
