
using WeatherCo.Domain;

namespace WeatherCo.Infrastructure;

public sealed class SystemDateProvider : IDateProvider
{
    public DateOnly TodayUtc() => DateOnly.FromDateTime(DateTime.UtcNow.Date);
}
