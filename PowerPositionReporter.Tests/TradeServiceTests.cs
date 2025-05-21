using Axpo;
using Moq;
using PowerPositionReporter.Models;
using PowerPositionReporter.Services;

namespace PowerPositionReporter.Tests
{
    public class TradeServiceTests
    {
        [Fact]
        public void GetTradesByPowerService_Returns_Correct_Trades()
        {
            // Arrange
            var date = new DateTime(2025, 5, 19);

            var powerTradeMock = new List<PowerTrade>
        {
            new PowerTrade
            {
                Periods = new[]
                {
                    new PowerPeriod{ Period = 1, Volume = 100.0 },
                    new PowerPeriod{ Period = 2, Volume = 150.0 }
                }
            }
        };

            var mockPowerService = new Mock<IPowerService>();
            mockPowerService.Setup(s => s.GetTrades(date))
                            .Returns((IEnumerable<Axpo.PowerTrade>)powerTradeMock);

            var tradeService = new TradeService(mockPowerService.Object);

            // Act
            var trades = tradeService.GetTradesByPowerService(date);

            // Assert
            var tradeList = new List<Trade>(trades);
            Assert.Equal(2, tradeList.Count);
            Assert.Equal(100.0, tradeList[0].Volume);
            Assert.Equal(150.0, tradeList[1].Volume);

            var expectedStart = date.Date.AddDays(-1).AddHours(23);
            Assert.Equal(expectedStart.ToUniversalTime(), tradeList[0].UtcTimestamp);
            Assert.Equal(expectedStart.AddHours(1).ToUniversalTime(), tradeList[1].UtcTimestamp);
        }
    }
}
