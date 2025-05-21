using Microsoft.Extensions.Logging;
using PowerPositionReporter.Models;

namespace PowerPositionReporter.Services
{
    public class ReportGenerator
    {
        private readonly ILogger<ReportGenerator> _logger;

        public ReportGenerator(ILogger<ReportGenerator> logger)
        {
            _logger = logger;
        }

        public void GenerateReport(IEnumerable<Trade> trades, string outputFolder, TimeZoneInfo londonTimeZone)
        {
            // Convert trades to local time and group by the local hour
            var localTrades = trades.Select(t =>
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(t.UtcTimestamp, londonTimeZone);
                return new
                {
                    HourStart = localTime.Date.AddHours(localTime.Hour),
                    t.Volume
                };
            });

            var grouped = localTrades
                .GroupBy(t => t.HourStart)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Volume));

            // Define the fixed order of hours from 23:00 to 22:00
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
            var start = nowLocal.Date.AddDays(-1).AddHours(23); // 23:00 previous day

            var orderedHours = Enumerable.Range(0, 24)
                .Select(i => start.AddHours(i))
                .ToList();

            var filename = $"PowerPosition_{nowLocal:yyyyMMdd_HHmm}.csv";
            var filepath = Path.Combine(outputFolder, filename);

            Directory.CreateDirectory(outputFolder);

            using var writer = new StreamWriter(filepath);
            writer.WriteLine("Local Time,Volume");

            foreach (var hour in orderedHours)
            {
                var timeLabel = hour.ToString("HH:mm");
                var volume = grouped.TryGetValue(hour, out var vol) ? vol : 0;
                writer.WriteLine($"{timeLabel},  {volume}");
            }

            _logger.LogInformation($"Report written to {filepath}");
        }
    }
}
