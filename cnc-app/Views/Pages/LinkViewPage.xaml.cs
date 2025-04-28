using APP.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace APP.Views.Pages
{
    /// <summary>
    /// LinkViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class LinkViewPage : INavigableView<LinkViewModel>
    {
        public LinkViewModel ViewModel { get; }
        public LinkViewPage(LinkViewModel linkViewModel)
        {
            this.ViewModel = linkViewModel;
            DataContext = this;
            InitializeComponent();
        }

    }
}
