using APP.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace APP.ViewModels.Windows;
public partial class NavigationViewModel : ViewModel
{
    [ObservableProperty]
    private ObservableCollection<NavigationItem> _MenuItems = [
        new NavigationItem("Home", PackIconKind.Home.ToString(), typeof(HomeViewPage)),
        new NavigationItem("Device", PackIconKind.Apps.ToString(), typeof(DeviceViewPage)),
        new NavigationItem("Broker", PackIconKind.ChevronDoubleUp.ToString(), typeof(LinkViewPage)),
        new NavigationItem("Adaptor", PackIconKind.Link.ToString(), typeof(AdaptorViewPage)),
    ];
}

public class NavigationItem
{
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
