using APP.Domain.Enums;
using APP.ViewModels.Pages;
using System.Windows.Controls;

namespace APP.Views.Forms
{
    public partial class DeviceFormView : UserControl
    {
        public DeviceFormViewModel ViewModel { get; }

        public DeviceFormView(DeviceFormViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.DataContext = this;
            InitializeComponent();

            ModelCombo.ItemsSource = Enum.GetValues(typeof(DeviceModelEnum)).Cast<DeviceModelEnum>();
            ModelCombo.SelectedIndex = (int)ViewModel.Model;

            KindCombo.ItemsSource = Enum.GetValues(typeof(DeviceKindEnum)).Cast<DeviceKindEnum>();
            KindCombo.SelectedIndex = (int)ViewModel.Kind;

        }
    }
}
