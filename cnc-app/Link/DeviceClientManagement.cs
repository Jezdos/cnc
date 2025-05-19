using APP.Domain;
using APP.Domain.Enums;
using Data.UnitOfWork;
using System.Collections.Concurrent;
using transport_common;

namespace APP.Services
{
    public class DeviceClientManagement(IUnitOfWork unitOfWork) : IDisposable
    {

        private event EventHandler<(long deviceId, ConnectStatus status)> _ChangeActionNotice = (sender, objetc) => { };
        public event EventHandler<(long deviceId, ConnectStatus status)> ChangeActionNotice
        {
            add { _ChangeActionNotice += value; }
            remove { _ChangeActionNotice -= value; }
        }

        private readonly ConcurrentDictionary<long, IDevice> _clients = new();

        public bool IsClientConnected(long key) => _clients.TryGetValue(key, out var client) && client.Status == ConnectStatus.CONNECTED;

        public bool TryGetClient(long key, out IDevice? client) => _clients.TryGetValue(key, out client);


        public async Task<bool> Submit(Device item)
        {
            IDevice? device = GenDeviceClient(item);
            if (device is not null)
            {
                return await this.Submit(device);
            }
            return false;
        }

        public async Task<bool> Submit(IDevice client)
        {
            this.Remove(client.GetKey());
            return await Task.FromResult(_clients.TryAdd(client.GetKey(), client));
        }

        public bool Remove(long? key)
        {
            if (key is null) return false;
            if (_clients.TryRemove(key.Value, out IDevice? removedValue))
            {
                if (removedValue is not null)
                {
                    removedValue.ConnectionStatusChanged -= (key, status) => _ChangeActionNotice.Invoke(key, status);
                    _ = removedValue.Destory();
                    return true;
                }
            }
            return false;
        }

        private IDevice? GenDeviceClient(Device item)
        {

            if (item is { DeviceId: not null, Model: DeviceModelEnum.AUTO })
            {
                IDevice? device = item.Kind.Instance(item);
                if (device is not null)
                {

                    device.ConnectionStatusChanged += (linkId, status) => _ChangeActionNotice.Invoke(device.GetKey(), status);

                    Task.Run(async () =>
                    {
                        await device.Init();
                        await device.Connect();
                    });
                }

                return device;
            }
            return null;
        }


        public async Task InitAsync()
        {
            var respository = unitOfWork.GetRepository<Device>();
            List<Device> list = [.. await respository.GetAllAsync()];
            foreach (var item in list) await Submit(item);
        }

        public void Dispose()
        {
            foreach (var key in _clients.Keys)
            {
                try { Remove(key); } catch { /* log if needed */ }
            }
            _clients.Clear();
        }
    }
}
