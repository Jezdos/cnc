using APP.Domain;
using Data.UnitOfWork;
using log4net;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using transport_common;
using transport_common.Command;
using Transport_MQTT;

namespace APP.Services
{
    public class AdaptorManagement(IUnitOfWork unitOfWork, FMqttClientManagement mqttManagement, DeviceClientManagement deviceManagement) : IDisposable
    {

        private readonly ILog logger = LogManager.GetLogger(nameof(AdaptorManagement));

        // 双向映射关系存储
        private readonly ConcurrentDictionary<long, HashSet<Topic>> _deviceToLinks = new();
        private readonly ConcurrentDictionary<long, HashSet<Topic>> _linkToDevices = new();

        private readonly ReaderWriterLockSlim _rwLock = new();

        /// <summary>
        /// 通过 deviceId 获取对应的 HashSet<Topic>
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private IEnumerable<Topic> GetDeviceLinks(long deviceId)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _deviceToLinks.TryGetValue(deviceId, out var links) ? links.ToImmutableHashSet() : Enumerable.Empty<Topic>();
            }
            finally { _rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// 通过 linkId 获取对应的 HashSet<Topic>
        /// </summary>
        /// <param name="linkId"></param>
        /// <returns></returns>
        private IEnumerable<Topic> GetLinkDevices(long linkId)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _linkToDevices.TryGetValue(linkId, out var devices) ? devices.ToImmutableHashSet() : Enumerable.Empty<Topic>();
            }
            finally { _rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// 通过 deviceId 向 mqttClient 发布信息 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task NoticeByDeviceAsync(long deviceId, string message)
        {
            foreach (var item in GetDeviceLinks(deviceId))
            {
                if (mqttManagement.TryGetClient(item.Id, out FMqttClient? client))
                {
                    if (client is not null)
                    {
                        await client.PublishAsync(item.TopicTelemetryStr, message);
                    }
                }
            }
            ;
        }

        /// <summary>
        /// 通过 linkId 向 deviceClient 发布信息 
        /// </summary>
        /// <param name="linkId"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task NoticeByLink(long linkId, string topic, string message)
        {
            foreach (var item in GetLinkDevices(linkId))
            {
                if (deviceManagement.TryGetClient(item.Id, out IDevice? device) && device is not null)
                {
                    // 通知设备执行器执行
                    CommandResponse response = await device.ExecuteCommandAsync(message);

                    if (mqttManagement.TryGetClient(linkId, out FMqttClient? client) && client is not null)
                    {
                        topic = topic.Replace("request", "response");
                        client.PublishAsync(topic, JsonSerializer.Serialize(response));
                    }
                }
            }
            ;

            await Task.CompletedTask;
        }


        public async Task Reload(bool flag = false)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _deviceToLinks.Clear();
                _linkToDevices.Clear();
                var respository = unitOfWork.GetRepository<Adaptor>();
                List<Adaptor> list = [.. await respository.GetAllAsync()];
                foreach (var item in list) await InitRelation(item);
                // 恢复订阅
                if (flag) InitSubscribe();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        private async Task InitRelation(Adaptor adaptor)
        {
            if (adaptor is { LinkId: not null, DeviceId: not null, TopicTelemetry: not null, TopicRpc: not null })
            {
                _deviceToLinks.AddOrUpdate(adaptor.DeviceId.Value, [new Topic(adaptor.TopicTelemetry, adaptor.TopicRpc, adaptor.LinkId.Value)],
                    (_, set) => { set.Add(new Topic(adaptor.TopicTelemetry, adaptor.TopicRpc, adaptor.LinkId.Value)); return set; });

                _linkToDevices.AddOrUpdate(adaptor.LinkId.Value, [new Topic(adaptor.TopicTelemetry, adaptor.TopicRpc, adaptor.DeviceId.Value)],
                    (_, set) => { set.Add(new Topic(adaptor.TopicTelemetry, adaptor.TopicRpc, adaptor.DeviceId.Value)); return set; });
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            // 初始化数据
            await Reload();
            // 开始事件监听
            mqttManagement.ChangeActionNotice += RecoverSubscribe;
            mqttManagement.MessageProcess += MessageProcess;
            InitSubscribe();
        }

        /// <summary>
        /// 初始化订阅
        /// </summary>
        public void InitSubscribe()
        {
            // 初始化订阅
            foreach (var item in _linkToDevices)
            {
                if (mqttManagement.TryGetClient(item.Key, out FMqttClient? client) && client is not null)
                {
                    try
                    {
                        foreach (var topic in item.Value)
                        {
                            client.SubscribeAsync(topic.TopicRpc);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("SubscribeAsync ERROR: link id : {0} , cause : {1}", item.Key, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 恢复订阅数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="param"></param>
        private async void RecoverSubscribe(object? sender, (long linkId, ConnectStatus status) param)
        {
            if (param.status == ConnectStatus.CONNECTED)
            {
                if (mqttManagement.TryGetClient(param.linkId, out FMqttClient? senderObj) && senderObj is not null)
                {
                    foreach (var item in GetLinkDevices(param.linkId))
                    {
                        await senderObj.SubscribeAsync(item.TopicRpc);
                    }
                }
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="param"></param>
        private async void MessageProcess(object? sender, (long linkId, string topic, string payload) param)
        {
            await NoticeByLink(param.linkId, param.topic, param.payload);
        }


        public void Dispose()
        {
            mqttManagement.ChangeActionNotice -= RecoverSubscribe;
            _deviceToLinks.Clear();
            _linkToDevices.Clear();
            _rwLock.Dispose();
        }


        class Topic(string topicTelemetryStr, string topicRpc, long id)
        {
            public string TopicTelemetryStr { get; set; } = topicTelemetryStr;
            public string TopicRpc { get; set; } = topicRpc;
            public long Id { get; set; } = id;
        }
    }


}
