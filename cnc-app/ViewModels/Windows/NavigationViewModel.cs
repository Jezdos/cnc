using APP.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.ViewModels.Windows;
public partial class NavigationViewModel : ViewModel
{
    [ObservableProperty]
    private ObservableCollection<NavigationItem> _MenuItems = [
        new NavigationItem("Home", PackIconKind.Home.ToString(), typeof(HomeViewPage)),
        new NavigationItem("Device", PackIconKind.Devices.ToString(), null),
        new NavigationItem("Broker", PackIconKind.LinkVariant.ToString(), typeof(LinkViewPage)),
    ];
}

public class NavigationItem { 
    public string Title { get; }
    public string Kind { get; }
    public Type Type { get; }

    public NavigationItem(string title, string kind, Type type)
    {
        Title = title;
        Kind = kind;
        Type = type;
    }
}
