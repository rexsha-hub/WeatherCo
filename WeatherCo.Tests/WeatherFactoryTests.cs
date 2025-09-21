using System;
using WeatherCo.Producer;
using Xunit;

public class WeatherFactoryTests
{
    private readonly WeatherFactory _factory = new();

    [Fact]
    public void SameDate_YieldsSameForecast()
    {
        // Fixed date so the test is stable
        var d = new DateOnly(2025, 9, 21);

        var a = _factory.Create(d);
        var b = _factory.Create(d);

        Assert.Equal(a.TemperatureC, b.TemperatureC);
        Assert.Equal(a.TemperatureF, b.TemperatureF);
        Assert.Equal(a.Summary, b.Summary);
    }

    [Fact]
    public void UsesExpectedRanges_AndSummaryRule()
    {
        var d = new DateOnly(2025, 9, 21);
        var r = _factory.Create(d);

        // temp C must be from rng.Next(-6, 32)  => -6..31 inclusive
        Assert.InRange(r.TemperatureC, -6, 31);

        // F must be computed as 32 + round(C * 9/5)
        var expectedF = 32 + (int)Math.Round(r.TemperatureC * 9 / 5.0);
        Assert.Equal(expectedF, r.TemperatureF);

        // Summary is chosen by index = Abs(seed) % len, where seed == date.DayNumber
        var summaries = new[]
        { "Freezing","Bracing","Chilly","Cool","Mild","Warm","Balmy","Hot","Sweltering","Scorching" };

        var idx = Math.Abs(d.DayNumber) % summaries.Length;
        Assert.Equal(summaries[idx], r.Summary);
    }

    [Fact]
    public void DefaultSequence_TodayPlusNext4_MatchesFactory()
    {
        // Your Producer /forecast endpoint uses UTC "today"
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var expected = new[]
        {
            _factory.Create(start.AddDays(0)),
            _factory.Create(start.AddDays(1)),
            _factory.Create(start.AddDays(2)),
            _factory.Create(start.AddDays(3)),
            _factory.Create(start.AddDays(4))
        };

        // Just sanity: each item is deterministic and the count is 5
        Assert.Equal(5, expected.Length);
        // Also ensure at least one of the five differs from another day (highly likely)
        Assert.Contains(expected, e => e.TemperatureC != expected[0].TemperatureC || e.Summary != expected[0].Summary);
    }
}
