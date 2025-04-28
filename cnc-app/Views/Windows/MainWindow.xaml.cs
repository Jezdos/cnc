using APP.ViewModels.Windows;
using Wpf.Ui;

namespace APP.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(
        MainWindowViewModel mainWindowViewModel,
        NavigationViewModel navigationViewModel,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService
    )
    {
        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

        this.MainWindowViewModel = mainWindowViewModel;
        this.NavigationViewModel = navigationViewModel;
        this.NavigationService = navigationService;
        DataContext = this;

        InitializeComponent();

        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        navigationService.SetNavigationControl(NavigationView);
        contentDialogService.SetDialogHost(RootContentDialog);
    }

    public MainWindowViewModel MainWindowViewModel { get; }
    public NavigationViewModel NavigationViewModel { get; }

    public INavigationService NavigationService { get; }

    private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is NavigationItem item)
        {
            if (item.Type is not null && this.IsLoaded) NavigationService.Navigate(item.Type);
        }
    }
}