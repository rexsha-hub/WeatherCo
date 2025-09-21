
namespace WeatherCo.Domain;

public sealed record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string Summary
);
