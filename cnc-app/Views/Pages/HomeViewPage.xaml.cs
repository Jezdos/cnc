using APP.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace APP.Views.Pages
{
    /// <summary>
    /// HomeViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class HomeViewPage : INavigableView<HomeViewModel>
    {
        public HomeViewModel ViewModel { get; }
        public HomeViewPage(HomeViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.DataContext = this;
            InitializeComponent();
        }
    }
}
