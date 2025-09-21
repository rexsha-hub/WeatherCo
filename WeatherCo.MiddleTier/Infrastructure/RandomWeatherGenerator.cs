
using WeatherCo.Domain;

namespace WeatherCo.Infrastructure;

public sealed class RandomWeatherGenerator : IWeatherGenerator
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing","Bracing","Chilly","Cool","Mild","Warm","Balmy","Hot","Sweltering","Scorching"
    };

    public WeatherForecast Create(DateOnly date)
    {
        int seed = date.DayNumber;
        var rng = new Random(seed);
        int tempC = rng.Next(-10, 41);
        int tempF = (int)Math.Round(tempC * 9.0 / 5.0 + 32);
        string summary = Summaries[rng.Next(Summaries.Length)];
        return new WeatherForecast(date, tempC, tempF, summary);
    }
}
