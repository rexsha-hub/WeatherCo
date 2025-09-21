
namespace WeatherCo.Domain;

public interface IWeatherGenerator
{
    WeatherForecast Create(DateOnly date);
}
