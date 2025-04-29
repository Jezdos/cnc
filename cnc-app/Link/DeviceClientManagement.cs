using APP.Domain;
using APP.Domain.Enums;
using Data.UnitOfWork;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public bool IsClientConnected(long key) => _clients.TryGetValue(key, out var client) && client.Status == ConnectStatus.CONNECTED;

        public bool TryGetClient(long key, out IDevice? client) => _clients.TryGetValue(key, out client);

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

        private IDevice? GenMqttClient(Device item)
        {

            if (item is { DeviceId: not null, Model: DeviceModelEnum.AUTO })
            {
                FMqttClient client = new FMqttClient(
                            linkId: item.LinkId.Value,
                            server: item.Host,
                            port: item.Port.Value,
                            clientId: item.ClientId,
                            username: item.Username,
                            password: item.Password,
                            keepAliveSeconds: item.KeepAlive);

                client.ConnectionStatusChanged += (linkId, status) => _ChangeActionNotice.Invoke(linkId, status);

                Task.Run(async () =>
                {
                    await client.Init();
                    await client.Connect();
                });

                return client;
            }
            return null;
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
