using APP.Domain.Views;
using APP.ViewModels.Pages.DeviceForm;
using APP.Views.Forms.DeviceForm;

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
    }
}
