using System.Net.Http.Json;

namespace IntegrationTests;

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class ApiTests(AppFixture fixture)
{
    private static readonly string[] ExpectedSummaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    [Test]
    public async Task GetWeatherForecast_ReturnsFiveValidForecasts()
    {
        var http = fixture.CreateHttpClient("apiservice");

        var forecasts = await http.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");

        await Assert.That(forecasts).IsNotNull();
        await Assert.That(forecasts!).Count().IsEqualTo(5);
        await Assert.That(forecasts.All(f => ExpectedSummaries.Contains(f.Summary))).IsTrue();
    }

    private record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
}
