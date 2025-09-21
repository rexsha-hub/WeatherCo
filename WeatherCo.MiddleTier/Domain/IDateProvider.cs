
namespace WeatherCo.Domain;

public interface IDateProvider
{
    DateOnly TodayUtc();
}
