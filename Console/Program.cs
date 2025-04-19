// See https://aka.ms/new-console-template for more information
using MQTTnet;
using Transport_MQTT;

Console.WriteLine("Hello, World!");

FMqttClient mqttClient = new FMqttClient(0 ,"127.0.0.1", username : "admin", password:"123456");

await mqttClient.Init();
await mqttClient.Connect();

while (true) { 
    Thread.Sleep(1000);
}
