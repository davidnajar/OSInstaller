using System.Text.Json;
using Microsoft.Extensions.Options;
using OSInstaller.Models;

namespace OSInstaller.Services;

public class SpecLoaderService
{
    private readonly ILogger<SpecLoaderService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _specDirectories;

    public SpecLoaderService(ILogger<SpecLoaderService> logger, IWebHostEnvironment environment, IOptions<InstallerOptions> options)
    {
        _logger = logger;
        _environment = environment;
        
        // Use CLI-provided directories if available
        if (options.Value.SpecDirectories != null && options.Value.SpecDirectories.Length > 0)
        {
            _specDirectories = options.Value.SpecDirectories;
        }
        // In development, use example-specs directory
        else if (_environment.IsDevelopment())
        {
            var exampleSpecsPath = Path.Combine(_environment.ContentRootPath, "example-specs");
            _specDirectories = new[] { exampleSpecsPath };
        }
        else
        {
            _specDirectories = new[]
            {
                "/usr/share/installer/spec.d",
                "/etc/installer/spec.d",
                "/var/lib/installer/spec.d"
            };
        }
    }

    public async Task<List<ContribSpec>> LoadAllSpecsAsync()
    {
        var specs = new List<ContribSpec>();

        foreach (var directory in _specDirectories)
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogInformation("Spec directory does not exist: {Directory}", directory);
                continue;
            }

            var files = Directory.GetFiles(directory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var spec = JsonSerializer.Deserialize<ContribSpec>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (spec != null)
                    {
                        specs.Add(spec);
                        _logger.LogInformation("Loaded spec from {File}: {ContribId}", file, spec.ContribId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load spec from {File}", file);
                }
            }
        }

        return specs.OrderBy(s => s.Priority).ToList();
    }
}
