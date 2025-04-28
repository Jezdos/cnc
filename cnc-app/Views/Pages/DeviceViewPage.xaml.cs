using APP.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace APP.Views.Pages
{
    /// <summary>
    /// LinkViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceViewPage : INavigableView<DeviceViewModel>
    {
        public DeviceViewModel ViewModel { get; }
        public DeviceViewPage(DeviceViewModel deviceViewModel)
        {
            this.ViewModel = deviceViewModel;
            DataContext = this;
            InitializeComponent();
        }

    }
}
