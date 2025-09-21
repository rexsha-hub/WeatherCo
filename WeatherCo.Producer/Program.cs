using Microsoft.AspNetCore.Mvc;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WeatherCo.Producer.IWeatherFactory, WeatherCo.Producer.WeatherFactory>();

var app = builder.Build();

app.UseCors();

var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5191";
app.Urls.Clear();
app.Urls.Add(url);

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// ---------- /forecast endpoints ----------
var grp = app.MapGroup("/forecast");

// GET /forecast  -> current day + next 4
grp.MapGet("",
(WeatherCo.Producer.IWeatherFactory factory) =>
{
    //  UTC "today" to match the rest of your code. 
    var start = DateOnly.FromDateTime(DateTime.UtcNow.Date); // <-- no AddDays(1)

    var list = Enumerable.Range(0, 5)
        .Select(i => factory.Create(start.AddDays(i)))
        .ToList();

    return Results.Ok(list);
})
.WithName("GetForecastDefault")
.Produces<List<WeatherCo.Producer.WeatherDto>>(StatusCodes.Status200OK);

// GET /forecast/range?startDate=<YYYY-MM-DD or DD/MM/YYYY>&days=5  ? list
grp.MapGet("range",
([FromQuery] string? startDate, [FromQuery] int? days, WeatherCo.Producer.IWeatherFactory factory) =>
{
    var count = (days is null or <= 0) ? 5 : days.Value;
    if (count is < 1 or > 14)
        return Results.BadRequest(new { error = "days must be between 1 and 14" });

    DateOnly start;
    if (string.IsNullOrWhiteSpace(startDate))
    {
        start = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    }
    else if (!TryParseDate(startDate, out start))
    {
        return Results.BadRequest(new { error = "Invalid startDate. Use yyyy-MM-dd or dd/MM/yyyy." });
    }

    var list = Enumerable.Range(0, count).Select(i => factory.Create(start.AddDays(i))).ToList();
    return Results.Ok(list);
})
.WithName("GetForecastRange")
.Produces<List<WeatherCo.Producer.WeatherDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

// GET /forecast/{date}  ? single item
grp.MapGet("{date}",
([FromRoute] string date, WeatherCo.Producer.IWeatherFactory factory) =>
{
    if (!TryParseDate(date, out var d))
        return Results.BadRequest(new { error = "Invalid date. Use yyyy-MM-dd or dd/MM/yyyy." });

    return Results.Ok(factory.Create(d));
})
.WithName("GetForecastForDate")
.Produces<WeatherCo.Producer.WeatherDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

// Parse helper (ISO + UK + dd-MM-yyyy)
 static bool TryParseDate(string? s, out DateOnly d)
{
    d = default;
    if (string.IsNullOrWhiteSpace(s)) return false;

    s = Uri.UnescapeDataString(s.Trim());

    // ISO first (safe in URLs)
    if (DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
        return true;

    // day-first + US fallback
    var fmts = new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy" };
    if (DateOnly.TryParseExact(s, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
        return true;

    // last resort: en-GB culture parse
    return DateOnly.TryParse(s, new CultureInfo("en-GB"), DateTimeStyles.None, out d);
}


app.Run();

// -------------------- Types below --------------------
namespace WeatherCo.Producer
{
    public record WeatherDto(DateOnly Date, int TemperatureC, int TemperatureF, string Summary);

    public interface IWeatherFactory
    {
        WeatherDto Create(DateOnly date);
    }

    public sealed class WeatherFactory : IWeatherFactory
    {
        private static readonly string[] Summaries = new[]
            { "Freezing","Bracing","Chilly","Cool","Mild","Warm","Balmy","Hot","Sweltering","Scorching" };

        public WeatherDto Create(DateOnly date)
        {
            var seed = date.DayNumber;
            var rng = new Random(seed);
            var c = rng.Next(-6, 32);
            var f = 32 + (int)Math.Round(c * 9 / 5.0);
            var summary = Summaries[Math.Abs(seed) % Summaries.Length];
            return new WeatherDto(date, c, f, summary);
        }
    }
}
