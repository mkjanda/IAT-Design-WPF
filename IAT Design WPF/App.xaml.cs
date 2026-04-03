using FluentValidation;
using System.Windows;
using Velopack;
using Microsoft.Extensions.DependencyInjection;
using IAT.Core.Services;
using IAT.ViewModels;
using System.CodeDom;
using IAT.Core.Models.Serializable;
using IAT.Core.Validation;
using IAT_Design_WPF.Services;

namespace IAT_Design_WPF
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 1. VelopackApp MUST run FIRST — this sets up the locator and is required
            VelopackApp.Build()
                .Run();

            // 2. Velopack update check (the correct modern API)
            var updateManager = new UpdateManager("https://github.com/mkjanda/IAT-Design-WPF/releases/latest");

            var updateInfo = await updateManager.CheckForUpdatesAsync();
            if (updateInfo != null)
            {
                // Optional: show a small "Updating..." window here later
                await updateManager.DownloadUpdatesAsync(updateInfo);

                // This applies the update and restarts the app cleanly
                updateManager.ApplyUpdatesAndRestart(updateInfo);
                return;   // important — stop startup if we're restarting
            }

            // 3. Dependency Injection setup (the professional way)
            var services = new ServiceCollection();

            // Register your services here as we build them
            services.AddSingleton<ILocalStorageService, LocalStorageService>();
            services.AddSingleton<IXmlDeserializationService, XmlDeserializationService>();
            services.AddSingleton<IStringResourceService, StringResourceService>();
            services.AddScoped<IValidator<Test>, TestValidator>();
            services.AddSingleton<IUserNotificationService, UserNotificationService>(); 
            services.AddTransient<ValidationService>();
            services.AddTransient<WebSocketService>();

            Services = services.BuildServiceProvider();

            base.OnStartup(e);

            // 4. Launch the main window with MVVM
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}