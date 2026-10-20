using Microsoft.Extensions.DependencyInjection;

namespace Weatherproj;

class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        await Task.CompletedTask; //заглушка чтоб компилятор не ругался
    }
}
