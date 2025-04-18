﻿using Core.Utils;
using log4net;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using transport_common;

namespace Transport_MQTT;
public class FMqttClient(long linkId, string server, int port = 1883, string? clientId = null, string? username = "", string? password = "", int? keepAliveSeconds = 10) : IConnectLifeCycle
{

    private static readonly ILog logger = LogManager.GetLogger(nameof(FMqttClient));

    private IMqttClient? _mqttClient;
    private MqttClientOptions? _channelOptions;

    private readonly long _linkId = linkId;
    private readonly string _clientId = clientId ?? $"Client_{SnowflakeIdWorker.Singleton.nextId()}";
    private readonly string _server = server;
    private readonly int _port = port;

    private readonly string _username = username ?? "";
    private readonly string _password = password ?? "";
    private readonly int _keepAliveSeconds = keepAliveSeconds ?? 10;

    public event EventHandler<ConnectStatus>? _ConnectionStatusChanged;
    public event EventHandler<ConnectStatus>? ConnectionStatusChanged
    {
        add => _ConnectionStatusChanged += value;
        remove => _ConnectionStatusChanged -= value;
    }

    public event EventHandler<(string Topic, string Payload)>? _MessageProcess;
    public event EventHandler<(string Topic, string Payload)>? MessageProcess
    {
        add => _MessageProcess += value;
        remove => _MessageProcess -= value;
    }

    public override async Task InitAsync()
    {
        _channelOptions = new MqttClientOptionsBuilder().WithClientId(_clientId)
            .WithTcpServer(_server, _port)
            .WithCredentials(_username, _password)
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_keepAliveSeconds))
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
            .Build();

        await Task.CompletedTask;
    }

    
    public override async Task ConnectAsync()
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
        // 设置事件处理程序
        _mqttClient.ConnectedAsync += OnConnected;
        _mqttClient.DisconnectedAsync += OnDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceived;

        await AttemptConnectWithRetry().ConfigureAwait(false);
    }

    private async Task AttemptConnectWithRetry()
    {
        if (_mqttClient == null) return;

        while (Status == ConnectStatus.CONNECTING)
        {
            try
            {
                await _mqttClient.ConnectAsync(_channelOptions).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.DebugFormat("MqttClient connection failed, try again in five seconds. ex:{0}", ex);
                await Task.Delay(5000);
            }
        }
    }

    // 订阅主题（支持QoS级别）
    public async Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.ExactlyOnce)
    {
        if (_mqttClient?.IsConnected != true)
        {
            logger.ErrorFormat("SubscribeAsync ERROR: client: {0} is not connected", _clientId);
            throw new InvalidOperationException("MQTT client is not connected");
        }

        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel(qos)
            .Build();

        await _mqttClient.SubscribeAsync(topicFilter).ConfigureAwait(false);
    }

    // 取消订阅
    public async Task UnsubscribeAsync(string topic)
    {
        if (_mqttClient?.IsConnected != true)
        {
            logger.ErrorFormat("UnsubscribeAsync ERROR: client: {0}  is not connected", _clientId);
            throw new InvalidOperationException("MQTT client is not connected");
        }

        await _mqttClient.UnsubscribeAsync(topic).ConfigureAwait(false);
    }

    // 发布消息（支持QoS级别和保留标志）
    public async Task PublishAsync(string topic, string payload,
        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce,
        bool retain = false)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
        {
            logger.ErrorFormat("PublishAsync ERROR: client: {0}  is not connected", _clientId);
            throw new InvalidOperationException("MQTT client is not connected");
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(retain)
            .Build();

        await _mqttClient.PublishAsync(message).ConfigureAwait(false);
    }

    private Task OnConnected(MqttClientConnectedEventArgs e)
    {
        if (_mqttClient?.IsConnected == true)
        {
            base.ChangeStatus(ConnectStatus.CONNECTED);
            logger.DebugFormat("client: {0} Connected to {1} : {2}", _clientId, _server, _port);
        }
        else
        {
            logger.DebugFormat("client: {0} Connected failed, cause {1}", _clientId, e.ConnectResult.ReasonString);
        }

        ConnectionStatusChanged?.Invoke(this, base.Status);
        return Task.CompletedTask;
    }

    private async Task OnDisconnected(MqttClientDisconnectedEventArgs e)
    {
        base.ChangeStatus(ConnectStatus.DISCONNECT);
        logger.DebugFormat("client: {0} Disconnected, cause : {1}", _clientId, e.Reason);
        ConnectionStatusChanged?.Invoke(this, base.Status);

        if (e.ClientWasConnected)
        {
            base.ChangeStatus(ConnectStatus.CONNECTING);
            await AttemptConnectWithRetry();
        }

    }

    private Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = !e.ApplicationMessage.Payload.IsEmpty
            ? Encoding.UTF8.GetString(e.ApplicationMessage.Payload)
            : string.Empty;
        logger.DebugFormat("client: {0} receive message : {1}", _clientId, payload);
        MessageProcess?.Invoke(this, (e.ApplicationMessage.Topic, payload));
        return Task.CompletedTask;
    }

    public override async Task DisconnectAsync()
    {
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync().ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        if (_mqttClient == null) return;

        // 解除事件绑定
        _mqttClient.ConnectedAsync -= OnConnected;
        _mqttClient.DisconnectedAsync -= OnDisconnected;
        _mqttClient.ApplicationMessageReceivedAsync -= OnApplicationMessageReceived;

        _mqttClient.Dispose();

        GC.SuppressFinalize(this);
    }
}
