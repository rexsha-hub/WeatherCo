using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using WeatherCo.Domain;

namespace WeatherCo.Presentation;

public class ForecastFunctions
{
    private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy" };
    private readonly IForecastService _svc;
    public ForecastFunctions(IForecastService svc) => _svc = svc;

    [Function("GetForecast")]
    public async Task<HttpResponseData> GetForecast(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecast")] HttpRequestData req)
    {
        var list = _svc.GetNextFive();
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(list);
        return res;
    }

    [Function("GetNextFive")]
    public async Task<HttpResponseData> GetNextFive(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecast/next")] HttpRequestData req)
    {
        var list = _svc.GetNextFive();
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(list);
        return res;
    }

    [Function("GetByDate")]
    public async Task<HttpResponseData> GetByDate(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecast/date/{*date}")]
    HttpRequestData req,
     string date)
    {
        if (!TryParseDate(date, out var d))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "Invalid date. Use yyyy-MM-dd or dd/MM/yyyy." });
            return bad;
        }

        try
        {
            var item = _svc.GetSingleRestricted(d);
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(item);
            return ok;
        }
        catch (ArgumentException ex)
        {
            // business-rule violation → 400 with message
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = ex.Message });
            return bad;
        }
    }

    [Function("GetRange")]
    public async Task<HttpResponseData> GetRange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecast/range")] HttpRequestData req)
    {
        var qs = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var startRaw = qs["startDate"] ?? qs["start"];
        var daysRaw = qs["days"];

        DateOnly start = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(1);
        if (!string.IsNullOrWhiteSpace(startRaw) && !TryParseDate(startRaw, out start))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "Invalid start date. Use yyyy-MM-dd or dd/MM/yyyy." });
            return bad;
        }

        int days = 5;
        if (!string.IsNullOrWhiteSpace(daysRaw) && !int.TryParse(daysRaw, out days))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "days must be an integer." });
            return bad;
        }
        if (days < 1 || days > 14)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "days must be between 1 and 14." });
            return bad;
        }

        var list = _svc.GetRange(start, days);
        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(list);
        return ok;
    }

    private static bool TryParseDate(string? s, out DateOnly d)
    {
        d = default;
        return !string.IsNullOrWhiteSpace(s) &&
        DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
    }
}
