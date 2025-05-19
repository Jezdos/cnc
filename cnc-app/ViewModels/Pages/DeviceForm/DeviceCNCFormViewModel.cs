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

        [Range(minimum: 1000, maximum: 99999, ErrorMessage = "请输入数字: 1000-99999")]
        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public int? _Port;
        partial void OnPortChanged(int? value) => ValidateProperty(value, nameof(Port));

        [Required(ErrorMessage = "该字段不能为空")]
        [ObservableProperty]
        public string? _Path;
        partial void OnPathChanged(string? value) => ValidateProperty(value, nameof(Path));

        public override void AutoFill(Device device)
        {
            Device = new DeviceCNC(device);
            this.Host = Device.Host;
            this.Port = Device.Port;
            this.Path = Device.Path;
        }

        public override string ReadParams()
        {
            if (Device is null) return string.Empty;
            Device.Host = this.Host;
            Device.Port = this.Port;
            Device.Path = this.Path;
            return Device.Serializer();
        }
    }
}
