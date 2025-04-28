using APP.Domain;
using CommunityToolkit.Mvvm.ComponentModel;

namespace APP.ViewModels.Pages.DeviceForm
{
    public abstract class IDeviceFormViewModel : ObservableValidator
    {
        public abstract void AutoFill(Device device);

        public abstract string ReadParams();
    }
}
