using OSInstaller.Components;
using OSInstaller.Models;
using OSInstaller.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse CLI arguments
var specDirs = new List<string>();
string? outputPath = null;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--spec-dir" && i + 1 < args.Length)
    {
        specDirs.Add(args[++i]);
    }
    else if (args[i] == "--output" && i + 1 < args.Length)
    {
        outputPath = args[++i];
    }
    else if (args[i] == "--help" || args[i] == "-h")
    {
        Console.WriteLine("OSInstaller - Dynamic Installer Wizard");
        Console.WriteLine();
        Console.WriteLine("Usage: OSInstaller [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --spec-dir <path>    Specification directory (can be specified multiple times)");
        Console.WriteLine("  --output <path>      Output JSON file path");
        Console.WriteLine("  --help, -h           Show this help message");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  OSInstaller --spec-dir /usr/share/installer/spec.d --spec-dir /etc/installer/spec.d --output /var/lib/installer/config.json");
        return;
    }
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add API controllers
builder.Services.AddControllers();

// Configure installer options
builder.Services.Configure<InstallerOptions>(options =>
{
    options.SpecDirectories = specDirs.Count > 0 ? specDirs.ToArray() : null;
    options.OutputPath = outputPath;
});

// Add installer services
builder.Services.AddSingleton<SpecLoaderService>();
builder.Services.AddSingleton<SpecMergerService>();
builder.Services.AddSingleton<WizardStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OSInstaller.Client._Imports).Assembly);

// Map API controllers
app.MapControllers();

app.Run();
