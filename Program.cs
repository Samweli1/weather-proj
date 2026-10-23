using System.Net;
using Microsoft.Extensions.DependencyInjection;
using weather_proj.Services;
using Microsoft.Extensions.Options;
using weather_proj.Configuration;
using Microsoft.Extensions.Configuration;

namespace weather_proj;

public class Program
{
    public static async Task Main(string[] args)
    
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection();
        
        services.AddOptions<AccuWeatherOptions>()
            .Bind(configuration.GetSection(AccuWeatherOptions.SectionName))
            .ValidateDataAnnotations()  
            .ValidateOnStart();


        services.AddHttpClient<IWeatherService, WeatherService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<ISettingsService, SettingsService>();
        

        using ServiceProvider provider = services.BuildServiceProvider();
        
        await Task.CompletedTask; //заглушка чтоб компилятор не ругался

    }

    
}
