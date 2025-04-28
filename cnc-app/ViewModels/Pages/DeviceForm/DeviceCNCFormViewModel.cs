using APP.Domain;
using APP.Domain.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace APP.ViewModels.Pages.DeviceForm
{
    public partial class DeviceCNCFormViewModel : IDeviceFormViewModel
    {
        private DeviceCNC? Device;

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public string? _Host;
        partial void OnHostChanged(string? value) => ValidateProperty(value, nameof(Host));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public long? _Port;
        partial void OnPortChanged(long? value) => ValidateProperty(value, nameof(Port));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public long? _ReadTimeOut;
        partial void OnReadTimeOutChanged(long? value) => ValidateProperty(value, nameof(ReadTimeOut));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public string? _Path;
        partial void OnPathChanged(string? value) => ValidateProperty(value, nameof(Path));

        public override void AutoFill(Device device)
        {
            Device = new DeviceCNC(device);
            this.Host = Device.Host;
            this.Port = Device.Port;
            this.ReadTimeOut = Device.ReadTimeOut;
            this.Path = Device.Path;
        }

        public override string ReadParams()
        {
            if (Device is null) return string.Empty;
            Device.Host = this.Host;
            Device.Port = this.Port;
            Device.ReadTimeOut = this.ReadTimeOut;
            Device.Path = this.Path;
            return Device.Serializer();
        }
    }
}
