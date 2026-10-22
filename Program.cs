using System.Net;
using Microsoft.Extensions.DependencyInjection;
using weather_proj.Services;

namespace weather_proj;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<HttpClient>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<IWeatherService, WeatherService>();

        using ServiceProvider provider = services.BuildServiceProvider();
        
        await Task.CompletedTask; //заглушка чтоб компилятор не ругался

    }

    
}
