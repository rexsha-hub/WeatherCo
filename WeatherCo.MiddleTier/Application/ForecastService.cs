
using WeatherCo.Domain;

namespace WeatherCo.Application;

public sealed class ForecastService : IForecastService
{
    private readonly IWeatherGenerator _generator;
    private readonly IDateProvider _clock;

    public ForecastService(IWeatherGenerator generator, IDateProvider clock)
    {
        _generator = generator;
        _clock = clock;
    }

    public IReadOnlyList<WeatherForecast> GetNextFive()
    {
        var start = _clock.TodayUtc();                 // today, not tomorrow
        return GetRange(start, 5);
    }

    public IReadOnlyList<WeatherForecast> GetRange(DateOnly start, int days)
    {
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be > 0");
        return Enumerable.Range(0, days)
            .Select(i => _generator.Create(start.AddDays(i)))
            .ToArray();
    }

    public WeatherForecast GetSingleRestricted(DateOnly date)
    {
        var start = _clock.TodayUtc();                 // allow today
        var last = start.AddDays(4);                  // today + 4
        if (date < start || date > last)
            throw new ArgumentException("Date must be within today and the next 4 days.", nameof(date));

        return _generator.Create(date);
    }
}
