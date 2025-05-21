using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerPositionReporter.Services;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

// Setup config
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
string? outputFolder = builder.Configuration["OutputFolder"];

// Setup logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Setup DI
builder.Services.AddSingleton<TradeService>();
builder.Services.AddSingleton<ReportGenerator>();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting Power Position Report Generation...");

    var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/London");

    var trades = host.Services.GetRequiredService<TradeService>().GetTradesForToday();
    var generator = host.Services.GetRequiredService<ReportGenerator>();

    if (args.Length > 0 && Directory.Exists(args[0]))
    {
        outputFolder = args[0];
    }

    if (string.IsNullOrEmpty(outputFolder))
        throw new Exception("Output folder not specified.");

    generator.GenerateReport(trades, outputFolder, londonTimeZone);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred generating the report.");
}

