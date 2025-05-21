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

        public IEnumerable<Trade> GetTradesByPowerService(DateTime date)
        {
            // 1. Instantiate the service class from PowerService
            var powerTrades =  _powerService.GetTrades(date);
            var trades = new List<Trade>();
           
            var start = date.Date.AddDays(-1).AddHours(23); // 23:00 on previous day

            foreach (var trade in powerTrades)
            {
                foreach (var period in trade.Periods)
                {
                    int periodNum = period.Period;
                    double volume = period.Volume;

                    DateTime localTimestamp = start.AddHours(periodNum - 1);
                    DateTime utcTimestamp = TimeZoneInfo.ConvertTimeToUtc(localTimestamp, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));

                    trades.Add(new Trade
                    {
                        UtcTimestamp = utcTimestamp,
                        Volume = volume
                    });
                }
            }

                return trades;

        }
    }
}
