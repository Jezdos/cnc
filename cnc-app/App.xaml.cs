using APP.Services;
using APP.ViewModels.Windows;
using APP.Views.Windows;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Core.Extensions;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
using Microsoft.Extensions.Logging;
using Data.Extensions;
using APP.Configuration;
using Data.Interceptor;
using log4net;
using System;

namespace APP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => c.SetBasePath(AppContext.BaseDirectory))
        .ConfigureServices(
            (_1, services) =>
            {
                _ = services.AddNavigationViewPageProvider();
                // App Host
                _ = services.AddHostedService<ApplicationHostService>();
                _ = services.AddHostedService<AppInitializeService>();

                // logging
                services.AddLogging((builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.AddLog4Net("./appdata/log4net.config");
                });

                services.AddDbContextPool<APPDbContext>((options) => options
                    .UseSqlite(ConfigurationUtil.Default.ReadConfiguration(AppConstants.CONFIG_DB_NAME),  mySqlOptions =>  mySqlOptions.CommandTimeout(30))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .AddInterceptors(GetRequiredService<MetaDateInterceptor>())
                    .LogTo(message => LogManager.GetLogger("EFCore").Debug(message), LogLevel.Debug), poolSize:5);

                services.AddUnitOfWork<APPDbContext>();

                _ = services.AddSingleton<MetaDateInterceptor>();

                // Main window container with navigation
                _ = services.AddSingleton<Window, MainWindow>();
                _ = services.AddSingleton<MainWindowViewModel, MainWindowViewModel>();
                _ = services.AddSingleton<NavigationViewModel, NavigationViewModel>();
                _ = services.AddSingleton<INavigationService, NavigationService>();
                _ = services.AddSingleton<ISnackbarService, SnackbarService>();
                _ = services.AddSingleton<IContentDialogService, ContentDialogService>();
                _ = services.AddSingleton<WindowsProviderService>();
                _ = services.AddSingleton<FMqttClientManagement>();

                _ = services.AddTransientFromNamespace("APP.Views.Pages", Assembly.GetExecutingAssembly());
                _ = services.AddTransientFromNamespace("APP.ViewModels.Pages", Assembly.GetExecutingAssembly());

                
            }
        )
        .Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetRequiredService<T>()
            where T : class
        {
            return _host.Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private void OnExit(object sender, ExitEventArgs e)
        {
            _host.StopAsync().Wait();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
        }
    }

}
