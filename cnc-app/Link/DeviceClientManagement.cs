using APP.Domain;
using Data.UnitOfWork;
using System.Collections.Concurrent;
using transport_common;
using Transport_MQTT;

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

        public bool IsClientConnected(long deviceId) => _clients.TryGetValue(deviceId, out var client) && client.Status == ConnectStatus.CONNECTED;

        public bool TryGetClient(long deviceId, out IDevice? client) => _clients.TryGetValue(deviceId, out client);

        public async Task<bool> Submit(IDevice client)
        {
            this.Remove(client.DeviceId);

            return await Task.FromResult(_clients.TryAdd(client.DeviceId, client));
        }


        public bool Remove(long? deviceId)
        {
            if (deviceId is null) return false;
            if (_clients.TryRemove(deviceId.Value, out IDevice? removedValue))
            {
                if (removedValue is not null)
                {
                    removedValue.ConnectionStatusChanged -= (linkId, status) => _ChangeActionNotice.Invoke(linkId, status);
                    _ = removedValue.Destory();
                    return true;
                }
            }
            return false;
        }

        public async Task InitAsync()
        {
            //var respository = unitOfWork.GetRepository<LinkMqtt>();
            //List<LinkMqtt> list = [.. await respository.GetAllAsync()];

            //foreach (var item in list)
            //{
            //    FMqttClient? client = await GenMqttClient(item);
            //    if (client != null) await Submit(item.LinkId.Value, client);
            //}
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
