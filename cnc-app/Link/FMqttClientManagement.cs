using APP.Domain;
using Data.UnitOfWork;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using transport_common;
using Transport_MQTT;

namespace APP.Services
{
    public class FMqttClientManagement(IUnitOfWork unitOfWork) : IHostedService
    {

        private static event Action<long, ConnectStatus> _ChangeActionNotice = (sender, objetc) => { };
        public static event Action<long, ConnectStatus> ChangeActionNotice
        {
            add { _ChangeActionNotice += value; }
            remove { _ChangeActionNotice -= value; }
        }

        private readonly ConcurrentDictionary<long, FMqttClient> _clients = new();

        public async Task<bool> Submit(LinkMqtt item)
        {
            FMqttClient? client = await GenMqttClient(item);
            if (client is not null) {
                return await this.Submit(item.LinkId.Value, client);
            }
            return false;
        }

        public async Task<bool> Submit(long linkId, FMqttClient client) {

            this.Remove(linkId);

            return await Task.FromResult(_clients.TryAdd(linkId, client));
        }
        

        public bool Remove(long? linkId) {
            if (linkId is null) return false;
            if ( _clients.TryRemove(linkId.Value, out FMqttClient? removedValue))
            {
                if (removedValue is not null) {
                    removedValue.Dispose();
                    return true;
                }
            }
            return false;
        }

        public bool IsClientConnected(long linkId)
        {
            return _clients.TryGetValue(linkId, out var client) && client.Status == ConnectStatus.CONNECTED;
        }

        public bool TryGetClient(long linkId, out FMqttClient? client) =>
            _clients.TryGetValue(linkId, out client);

        public IEnumerable<FMqttClient> GetAllClients() => _clients.Values;

        public IEnumerable<FMqttClient> GetConnectedClients() =>
            _clients.Values.Where(c => c.Status == ConnectStatus.CONNECTED);

        public void DisposeAll()
        {
            foreach (var client in _clients.Values)
            {
                try { client.Dispose(); } catch { /* log if needed */ }
            }

            _clients.Clear();
        }

        private static async Task<FMqttClient?> GenMqttClient(LinkMqtt item) {

            if (item is { LinkId: not null, Host: not null, Port: not null })
            {
                FMqttClient client = new FMqttClient(
                            linkId: item.LinkId.Value,
                            server: item.Host,
                            port: item.Port.Value,
                            clientId: item.ClientId,
                            username: item.Username,
                            password: item.Password,
                            keepAliveSeconds: item.KeepAlive);

                await client.InitAsync();
                await client.ConnectAsync();

                return client;
            }
            return null;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var respository = unitOfWork.GetRepository<LinkMqtt>();
            List<LinkMqtt> list = [.. await respository.GetAllAsync()];

            foreach (var item in list)
            {   
                FMqttClient? client = await GenMqttClient(item);
                if (client != null) await Submit(item.LinkId.Value, client);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _clients)
            {
                if (item.Value is not null) _ = item.Value.Destory();
            }

            return Task.CompletedTask;
        }
    }
}
