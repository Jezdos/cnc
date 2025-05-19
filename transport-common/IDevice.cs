using transport_common.Command;

namespace transport_common
{
    public abstract class IDevice(int interval = 5000) : CommandRouter
    {
        public readonly int Interval = interval;

        public abstract string DeviceName { get; }

        protected abstract Task<Dictionary<string, object>> PollAsync();

        public async Task<object> Poll()
        {
            Dictionary<string, object> dataMap = await this.PollAsync();
            dataMap.Add("ConnectStatus", this.Status.ToString());
            return await Task.FromResult(dataMap);
        }
    }
}
