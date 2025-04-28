using APP.Domain;
using Data.UnitOfWork;
using System.Collections.Concurrent;
using transport_common;
using Transport_MQTT;

namespace APP.Services
{
    public class DeviceClientManagement(IUnitOfWork unitOfWork) : IDisposable
    {

        private event EventHandler<(long linkId, ConnectStatus status)> _ChangeActionNotice = (sender, objetc) => { };
        public event EventHandler<(long linkId, ConnectStatus status)> ChangeActionNotice
        {
            add { _ChangeActionNotice += value; }
            remove { _ChangeActionNotice -= value; }
        }

        private readonly ConcurrentDictionary<long, FMqttClient> _clients = new();

        public bool IsClientConnected(long linkId) => _clients.TryGetValue(linkId, out var client) && client.Status == ConnectStatus.CONNECTED;

        public bool TryGetClient(long linkId, out FMqttClient? client) => _clients.TryGetValue(linkId, out client);

        public async Task<bool> Submit(LinkMqtt item)
        {
            FMqttClient? client = await GenMqttClient(item);
            if (client is not null)
            {
                return await this.Submit(item.LinkId.Value, client);
            }
            return false;
        }

        public async Task<bool> Submit(long linkId, FMqttClient client)
        {

            this.Remove(linkId);

            return await Task.FromResult(_clients.TryAdd(linkId, client));
        }


        public bool Remove(long? linkId)
        {
            if (linkId is null) return false;
            if (_clients.TryRemove(linkId.Value, out FMqttClient? removedValue))
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


        private async Task<FMqttClient?> GenMqttClient(LinkMqtt item)
        {

            if (item is { LinkId: not null, Host: not null, Port: not null, Model: LinkModelEnum.AUTO })
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
            var respository = unitOfWork.GetRepository<LinkMqtt>();
            List<LinkMqtt> list = [.. await respository.GetAllAsync()];

            foreach (var item in list)
            {
                FMqttClient? client = await GenMqttClient(item);
                if (client != null) await Submit(item.LinkId.Value, client);
            }
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
