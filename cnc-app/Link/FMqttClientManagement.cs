using APP.Domain;
using Data.UnitOfWork;
using System.Collections.Concurrent;
using transport_common;
using Transport_MQTT;

namespace APP.Services
{
    public class FMqttClientManagement(IUnitOfWork unitOfWork) : IDisposable
    {
        // 状态处理
        private event EventHandler<(long linkId, ConnectStatus status)> _ChangeActionNotice = (sender, objetc) => { };
        public event EventHandler<(long linkId, ConnectStatus status)> ChangeActionNotice
        {
            add { _ChangeActionNotice += value; }
            remove { _ChangeActionNotice -= value; }
        }

        // 消息处理
        private event EventHandler<(long linkId, string Topic, string Payload)> _MessageProcess = (sender, objetc) => { };
        public event EventHandler<(long linkId, string Topic, string Payload)> MessageProcess
        {
            add => _MessageProcess += value;
            remove => _MessageProcess -= value;
        }

        private readonly ConcurrentDictionary<long, FMqttClient> _clients = new();

        public bool IsClientConnected(long linkId) => _clients.TryGetValue(linkId, out var client) && client.Status == ConnectStatus.CONNECTED;

        public bool TryGetClient(long linkId, out FMqttClient? client) => _clients.TryGetValue(linkId, out client);

        public async Task<bool> Submit(LinkMqtt item)
        {
            FMqttClient? client = GenMqttClient(item);
            if (client is not null)
            {
                return await this.Submit(client);
            }
            return false;
        }

        public async Task<bool> Submit(FMqttClient client)
        {
            this.Remove(client.GetKey());
            return await Task.FromResult(_clients.TryAdd(client.GetKey(), client));
        }


        public bool Remove(long? linkId)
        {
            if (linkId is null) return false;
            if (_clients.TryRemove(linkId.Value, out FMqttClient? removedValue))
            {
                if (removedValue is not null)
                {
                    removedValue.ConnectionStatusChanged -= _ChangeActionNotice.Invoke;
                    removedValue.MessageProcess -= _MessageProcess.Invoke;
                    _ = removedValue.Destory();
                    return true;
                }
            }
            return false;
        }

        public async Task InitAsync()
        {
            var respository = unitOfWork.GetRepository<LinkMqtt>();
            List<LinkMqtt> list = [.. await respository.GetAllAsync()];

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


        private FMqttClient? GenMqttClient(LinkMqtt item)
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

                client.ConnectionStatusChanged += (sender, data) => _ChangeActionNotice.Invoke(sender, data);
                client.MessageProcess += (sender, data) => _MessageProcess.Invoke(sender, data);

                Task.Delay(5000).ContinueWith(async (task) =>
                {
                    await client.Init();
                    await client.Connect();
                });

                return client;
            }
            return null;
        }
    }
}
