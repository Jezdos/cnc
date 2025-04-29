using System.Net.NetworkInformation;

namespace transport_common
{
    public abstract class IDevice: IConnectLifeCycle
    {
        public abstract string DeviceName { get; }

        public abstract Task<string> Collect();
    }
}
