
namespace WeatherCo.Domain;

public interface IForecastService
{
    /// Returns inclusive range [start, start + days - 1].
    IReadOnlyList<WeatherForecast> GetRange(DateOnly start, int days);

    /// Business rule: only allow tomorrow through tomorrow + 4.
    WeatherForecast GetSingleRestricted(DateOnly date);

    /// Next 5 days starting tomorrow.
    IReadOnlyList<WeatherForecast> GetNextFive();
}
