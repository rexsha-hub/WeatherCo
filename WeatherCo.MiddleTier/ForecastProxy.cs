using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace WeatherCo.Presentation;

public sealed class ForecastProxy
{
    private static readonly HttpClient Http = new HttpClient();

    // Capture any extra path; if none, we’ll forward to /forecast by default
    [Function("ProxyForecast")]
    public async Task<HttpResponseData> ProxyForecast(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecast-proxy/{*path}")]
        HttpRequestData req,
        string? path)
    {
        var baseUrl = Environment.GetEnvironmentVariable("PRODUCER_BASEURL") ?? "http://localhost:5192";

        // Default to /forecast when no path is supplied 
        var forwardPath = string.IsNullOrWhiteSpace(path) ? "/forecast"
                         : path.StartsWith("/") ? path
                         : "/" + path;

        var target = new Uri(new Uri(baseUrl), forwardPath + req.Url.Query);

        using var upstream = await Http.GetAsync(target, HttpCompletionOption.ResponseHeadersRead);

        var res = req.CreateResponse((HttpStatusCode)upstream.StatusCode);

        // pass through content headers
        foreach (var h in upstream.Headers)
            res.Headers.TryAddWithoutValidation(h.Key, string.Join(",", h.Value));
        foreach (var h in upstream.Content.Headers)
            res.Headers.TryAddWithoutValidation(h.Key, string.Join(",", h.Value));

        var bytes = await upstream.Content.ReadAsByteArrayAsync();
        await res.WriteBytesAsync(bytes);
        return res;
    }
}
