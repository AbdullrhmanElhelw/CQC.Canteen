using CQC.Canteen.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CQC.Canteen.UI;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }
    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // بتسجل كل حاجة (Domain, Data, UI)
                services.RegisterAppServices(hostContext.Configuration);
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        // 1. جهّز الداتابيز (Migrate + Seed)
        var serviceProvider = AppHost.Services;
        await DatabaseInitializer.InitializeDatabaseAsync(serviceProvider);

        // 2. افتح الشاشة الرئيسية (MainWindow)
        var startupForm = serviceProvider.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }
}

