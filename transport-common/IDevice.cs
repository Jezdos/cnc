using System.Net.NetworkInformation;

namespace transport_common
{
    public interface IDevice
    {
        string DeviceName { get; }

        long DeviceId { get; }

        ConnectStatus Status { get; }

        Task<string> Collect();
    }
}
