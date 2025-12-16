using System.Text.Json;
using OSInstaller.Models;

namespace OSInstaller.Services;

public class WizardStateService
{
    private readonly ILogger<WizardStateService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _stateFilePath;
    private WizardState? _cachedState;

    public WizardStateService(ILogger<WizardStateService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        
        // In development, use local directory
        if (_environment.IsDevelopment())
        {
            _stateFilePath = Path.Combine(_environment.ContentRootPath, "wizard-state.json");
        }
        else
        {
            _stateFilePath = "/var/lib/installer/wizard-state.json";
        }
        
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create state directory: {Directory}", directory);
            }
        }
    }

    public async Task<WizardState> LoadStateAsync()
    {
        if (_cachedState != null)
        {
            return _cachedState;
        }

        if (File.Exists(_stateFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_stateFilePath);
                var state = JsonSerializer.Deserialize<WizardState>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (state != null)
                {
                    _cachedState = state;
                    _logger.LogInformation("Loaded wizard state from {Path}", _stateFilePath);
                    return state;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load wizard state from {Path}", _stateFilePath);
            }
        }

        _cachedState = new WizardState();
        return _cachedState;
    }

    public async Task SaveStateAsync(WizardState state)
    {
        try
        {
            state.LastUpdated = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_stateFilePath, json);
            _cachedState = state;
            _logger.LogInformation("Saved wizard state to {Path}", _stateFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save wizard state to {Path}", _stateFilePath);
            throw;
        }
    }

    public async Task ClearStateAsync()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                File.Delete(_stateFilePath);
            }
            _cachedState = null;
            _logger.LogInformation("Cleared wizard state");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear wizard state");
            throw;
        }
    }
}
