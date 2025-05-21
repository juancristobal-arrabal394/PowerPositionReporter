using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerPositionReporter;
using PowerPositionReporter.Services;
using System.Runtime.InteropServices;

public class ReportWorker : BackgroundService
{
    private readonly ILogger<ReportWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _intervalInMinutes;
    private readonly string _outputfolder;

    public ReportWorker(ILogger<ReportWorker> logger, IServiceProvider serviceProvider, IOptions<Settings> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _intervalInMinutes = options.Value.IntervalInMinutes;
        _outputfolder = options.Value.OutputFolder;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Power Position Reporter started.");
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/London");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tradeService = scope.ServiceProvider.GetRequiredService<TradeService>();
                    var reportGenerator = scope.ServiceProvider.GetRequiredService<ReportGenerator>();

                    var reportDate = DateTime.UtcNow;
                    var trades = tradeService.GetTradesForToday(reportDate);
                    reportGenerator.GenerateReport(trades, _outputfolder, londonTimeZone);
                }

                _logger.LogInformation("Report generated at: {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalInMinutes), stoppingToken);
        }
    }
}

