namespace transport_common
{
    public interface IDevice
    {
        string Name { get; }

        long DeviceId { get; }

        Task<string> Collect();
    }
}
