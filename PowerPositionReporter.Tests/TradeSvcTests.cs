using Moq;
using Axpo;
using PowerPositionReporter.Services;

public class TradeServiceTests
{
    [Fact]
    public void GetTradesByPowerSvc_ReturnsAggregatedVolumes_PerPeriod()
    {
        // Arrange
        var date = new DateTime(2024, 12, 20);

        var trade1 = new PowerTrade
        {
            Periods = Enumerable.Range(1, 23)
                        .Select(i => new PowerPeriod { Period = i, Volume = 100 }).ToArray(),
        };

        var trade2 = new PowerTrade
        {
            Periods = Enumerable.Range(1, 23)
                        .Select(i =>
                        {
                            double volume = i switch
                            {
                                1 => 50,
                                2 => 40,
                                3 => -10,
                                22 => -20,
                                23 => -20,
                                _ => 100
                            };
                            return new PowerPeriod { Period = i, Volume = volume };
                        }).ToArray(),
        };

        var powerTrades = new List<PowerTrade> { trade1, trade2 };

        var mockPowerService = new Mock<IPowerService>();
        mockPowerService.Setup(ps => ps.GetTrades(date)).Returns((IEnumerable<Axpo.PowerTrade>)powerTrades);

        var service = new TradeService((Axpo.IPowerService)mockPowerService.Object);

        // Act
        var result = service.GetTradesByPowerSvc(date).ToList();

        // Assert
        Assert.Equal(23, result.Count);

        // 23:00 (period 1) => 100 + 50 = 150
        Assert.Equal(150, result[0].Volume);

        // 00:00 (period 2) => 100 + 40 = 140
        Assert.Equal(140, result[1].Volume);

        // 01:00 (period 3) => 100 - 10 = 90
        Assert.Equal(90, result[2].Volume);

        // Check 21:00 (period 22) => 100 - 20 = 80
        Assert.Equal(80, result[21].Volume);

        // Check 22:00 (period 23) => 100 - 20 = 80
        Assert.Equal(80, result[22].Volume);
    }
}
