using APP.ViewModels.Pages.DeviceForm;
using System.Windows.Controls;

namespace APP.Views.Forms.DeviceForm
{
    public partial class DeviceCNCFormView : UserControl, IDeviceFormView
    {
        public DeviceCNCFormViewModel ViewModel { get; }

        public DeviceCNCFormView(DeviceCNCFormViewModel ViewModel)
        {
            this.ViewModel = ViewModel;
            this.DataContext = this;
            InitializeComponent();
        }

        public string ReadParams()
        {
            return ViewModel.ReadParams();
        }
    }
}
