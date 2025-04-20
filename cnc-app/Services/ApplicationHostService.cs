// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using APP.Views.Pages;
using APP.Views.Windows;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Wpf.Ui.Appearance;


namespace APP.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public class ApplicationHostService(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        private MainWindow? _mainWindow;

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
            await CustomTheme();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private Task HandleActivationAsync()
        {
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                return Task.CompletedTask;
            }

            Window mainWindow = _serviceProvider.GetRequiredService<Window>();
            mainWindow.Loaded += OnMainWindowLoaded;
            mainWindow?.Show();

            return Task.CompletedTask;
        }


        private Task CustomTheme() {
            SystemThemeWatcher.UnWatch(Application.Current.MainWindow);
            ApplicationThemeManager.ApplySystemTheme(false);
            ApplicationThemeManager.Apply(ApplicationTheme.Light, updateAccent:false);
            return Task.CompletedTask;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not MainWindow mainWindow)
            {
                return;
            }

            _ = mainWindow.NavigationView.Navigate(typeof(HomeViewPage));
        }
    }
}
