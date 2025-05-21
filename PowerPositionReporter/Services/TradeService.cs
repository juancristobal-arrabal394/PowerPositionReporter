using PowerPositionReporter.Models;
using System.Reflection;
using Axpo;



namespace PowerPositionReporter.Services
{
    public class TradeService
    {
        public IEnumerable<Trade> GetTradesForToday()
        {
            var now = DateTime.UtcNow;
            var start = now.Date.AddDays(-1).AddHours(23); // Start at 23:00 of previous day
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

        public async Task<IEnumerable<Trade>> GetTradesForTodayExternal(DateTime date)
        {
            // 1. Instantiate the service class from PowerService
            var powerService = new PowerService();
            var powertrade = await powerService.GetTradesAsync(date);
            var period1 = powertrade.FirstOrDefault();
            var assembly = Assembly.LoadFrom("PowerService.dll");
            var powerServiceType = assembly.GetType("PowerService.PowerService");
            var method = powerServiceType?.GetMethod("GetTradesAsync");
            var instance = Activator.CreateInstance(powerServiceType!);
            var task = (Task<IEnumerable<object>>)method!.Invoke(instance, new object[] { date })!;
            var powerTrades = await task;

            var tradesext = new List<Trade>();
            var trades = new List<Trade>();
            var start = date.Date.AddDays(-1).AddHours(23); // 23:00 on previous day

            foreach (dynamic tra in powertrade)
            {
                foreach (dynamic pe in tra.Periods)
                {


                    int periodNum = pe.Period;
                    double volume = pe.Volume;
                    DateTime timestamp = start.AddHours(periodNum - 1);
                    tradesext.Add(new Trade { UtcTimestamp = timestamp.ToUniversalTime(), Volume = volume });
                }
            }

                foreach (dynamic powerTrade in powerTrades)
            {
                foreach (dynamic period in powerTrade.Periods)
                {
                    int periodNum = period.Period;
                    double volume = period.Volume;
                    DateTime timestamp = start.AddHours(periodNum - 1);
                    trades.Add(new Trade { UtcTimestamp = timestamp.ToUniversalTime(), Volume = volume });
                }
            }

            return tradesext;

        }
    }
}
