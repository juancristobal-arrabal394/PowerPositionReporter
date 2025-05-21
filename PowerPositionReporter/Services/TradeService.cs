using Axpo;
using PowerPositionReporter.Models;



namespace PowerPositionReporter.Services
{
    public class TradeService
    {
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
                    Volume = (int)Math.Round(random.NextDouble() * 100, 0)
                };

            }
        }

        public async Task<IEnumerable<Trade>> GetTradesForTodayExternal(DateTime date)
        {
            // 1. Instantiate the service class from PowerService
            var powerService = new PowerService();
            var powertrade = await powerService.GetTradesAsync(date);
            var tradesext = new List<Trade>();
           
            var start = date.Date.AddDays(-1).AddHours(23); // 23:00 on previous day

            foreach (dynamic tra in powertrade)
            {
                foreach (dynamic pe in tra.Periods)
                {

                    int periodNum = pe.Period;
                    int volume = pe.Volume;
                    DateTime timestamp = start.AddHours(periodNum - 1);
                    tradesext.Add(new Trade { UtcTimestamp = timestamp.ToUniversalTime(), Volume = volume });
                }
            }
                        
            return tradesext;

        }
    }
}
