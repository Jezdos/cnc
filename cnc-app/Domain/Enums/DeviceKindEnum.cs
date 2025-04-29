using APP.Domain.Views;
using APP.ViewModels.Pages.DeviceForm;
using APP.Views.Forms.DeviceForm;
using transport_cnc;
using transport_common;

namespace APP.Domain.Enums
{
    public enum DeviceKindEnum
    {
        CNC, CUTTER
    }

    public static class DeviceKindEnumExtensions
    {
        public static (Type Domain, Type Model, Type View) Descript(this DeviceKindEnum device)
        {
            return device switch
            {
                DeviceKindEnum.CNC => (typeof(DeviceCNC), typeof(DeviceCNCFormViewModel), typeof(DeviceCNCFormView)),
                DeviceKindEnum.CUTTER => (typeof(DeviceCNC), typeof(DeviceCNCFormViewModel), typeof(DeviceCNCFormView)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public static IDevice? Instance(this DeviceKindEnum kind, Device item)
        {
            
            switch (kind) { 
                case DeviceKindEnum.CNC:
                    { 
                        DeviceCNC device = new DeviceCNC(item);
                        if (device is { DeviceId: not null, Host: not null, Port: not null }) {
                            return new CncClient(device.DeviceId.Value, device.Host, device.Port.Value, device.Path);
                        }
                    }
                    break;
                case DeviceKindEnum.CUTTER:
                default: break;

            }
            return null;
}
    }
}
