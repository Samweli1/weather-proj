using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using weather_proj.Configuration;
using weather_proj.Services;
using weather_proj.UI;


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


        services.AddSingleton<ISettingsService, SettingsService>();
        
        services.AddHttpClient<IWeatherService, WeatherService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        
        services.AddTransient<ConsoleMenu>();

        
        using ServiceProvider provider = services.BuildServiceProvider();

        var menu = provider.GetRequiredService<ConsoleMenu>();
        await menu.RunAsync();
    }
}