# PowerPositionReporter
Power position reporter challenge.
.net core 8 / c#
Main entry program.cs. It use  GetTradesByPowerSvc(DateTime date) in TradeService.cs to retrieve trades from PowerService.dll interface using is method GetTrades. But it looks PowerService is in test method and it doesn't return the two array, it return a error response. So I've used a method, GetTradesForToday(DateTime date), to display the "tests" trade output. 
You can find a unit test project with the method GetTradesByPowerSvc_ReturnsAggregatedVolumes_PerPeriod(), to test how 2 trades periods are added correctly.
To run at intervall, using Task Scheduler and create a new task for the .exe that you obtain after running "dotnet publish -c Release -r win-x64 --self-contained true -o ./publish", so run directly without installing .NET.
To further refine this, consider daylight saving time. Use UTC and only use London time in reports, keeping in mind that the last Sunday in March would be 1:59 a.m. -> 3:00 a.m. and the last Sunday in October would be 2:59 a.m. -> 2:00 a.m.

