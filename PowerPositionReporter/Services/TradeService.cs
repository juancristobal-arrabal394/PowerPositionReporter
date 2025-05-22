using Axpo;
using PowerPositionReporter.Models;

namespace PowerPositionReporter.Services
{
    public class TradeService
        {
        private readonly IPowerService _powerService;
        public TradeService(IPowerService powerService)
        {
            _powerService = powerService;
        }
        public IEnumerable<Trade> GetTradesForToday(DateTime date)
        {
           // var now = DateTime.UtcNow;
            var start = date.Date.AddDays(-1).AddHours(23); // Start at 23:00 of previous day
            var random = new Random();
            for (int i = 0; i < 24; i++)
            {
                yield return new Trade
                {
                    UtcTimestamp = start.AddHours(i),
                    Volume = Math.Round(random.NextDouble() * 100, 0)
                };

            }
        }

        
        public IEnumerable<Trade> GetTradesByPowerSvc(DateTime date)
        {
            var trades = new List<Trade>();
            var powerTrades = _powerService.GetTrades(date);

            // Start time is 23:00 of the previous day in local time
            var localStart = TimeZoneInfo.ConvertTimeToUtc(date.Date.AddDays(-1).AddHours(23),
                TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

            var periodVolumes = new Dictionary<int, double>();

            for (int i = 1; i <= 23; i++)
            {
                periodVolumes[i] = 0;
            }

            foreach (var trade in powerTrades)
            {
                foreach (var period in trade.Periods)
                {
                    if (periodVolumes.ContainsKey(period.Period))
                    {
                        periodVolumes[period.Period] += period.Volume;
                    }
                }
            }

            // Create trade objects from aggregated periods
            foreach (var kvp in periodVolumes.OrderBy(p => p.Key))
            {
                int period = kvp.Key;
                double volume = kvp.Value;

                DateTime localTime = date.Date.AddDays(-1).AddHours(22 + period); // 23 + (period - 1)

                var utcTimestamp = TimeZoneInfo.ConvertTimeToUtc(localTime, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

                trades.Add(new Trade
                {
                    UtcTimestamp = utcTimestamp,
                    Volume = volume
                });
            }

            return trades;
        }

    }
}
