var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// UI reads middle tier base url from env var to avoid CORS headaches
var middleTierBase = Environment.GetEnvironmentVariable("MIDDLE_TIER_BASEURL") ?? "http://localhost:7071/api";

app.MapGet("/api/forecast", async (HttpContext ctx, HttpClient http) =>
{
    var date = ctx.Request.Query["date"].FirstOrDefault();
    var url = string.IsNullOrWhiteSpace(date) ? $"{middleTierBase}/forecast" : $"{middleTierBase}/forecast?date={date}";
    var upstream = await http.GetAsync(url);
    var body = await upstream.Content.ReadAsStringAsync();
    // Fix: Use Results.Content overload that accepts statusCode directly
    return Results.Content(body, upstream.Content.Headers.ContentType?.ToString() ?? "application/json", null, (int)upstream.StatusCode);
});

app.Run("http://localhost:5080");
