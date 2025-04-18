// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace APP.ViewModels.Windows;

public partial class MainWindowViewModel() : ViewModel
{

    [ObservableProperty]
    private String _applicationTitle = "CNC CONTROL PANEL";

    [ObservableProperty]
    private ObservableCollection<object> _menuItems = [];

    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems = [];

    [ObservableProperty]
    private ObservableCollection<Wpf.Ui.Controls.MenuItem> _trayMenuItems =
    [
        new Wpf.Ui.Controls.MenuItem { Header = "Home", Tag = "tray_home" },
        new Wpf.Ui.Controls.MenuItem { Header = "Close", Tag = "tray_close" },
    ];
}
