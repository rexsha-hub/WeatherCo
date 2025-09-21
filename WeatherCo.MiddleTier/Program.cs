
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WeatherCo.Domain;
using WeatherCo.Infrastructure;
using WeatherCo.Application;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IDateProvider, SystemDateProvider>();
        services.AddSingleton<IWeatherGenerator, RandomWeatherGenerator>();
        services.AddScoped<IForecastService, ForecastService>();
    })
    .Build();

host.Run();
