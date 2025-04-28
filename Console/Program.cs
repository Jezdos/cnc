// See https://aka.ms/new-console-template for more information
using transport_cnc;

Console.WriteLine("Hello, World!");

//FMqttClient mqttClient = new FMqttClient(0 ,"127.0.0.1", username : "admin", password:"123456");

//await mqttClient.Init();
//await mqttClient.Connect();

CncClient client = new CncClient(0L, "192.168.1.1", 8193, "//CNC_MEM/USER/PATH1/");
client.Init();
await client.Connect();

Console.WriteLine(client.Status);

//client.Collect();
bool flag = await client.WriteProcessFile(3333, "C:\\Users\\zzz\\Desktop\\O3333.TXT");

Console.WriteLine(flag);

while (true)
{
    Console.WriteLine(await client.Collect());
    Thread.Sleep(5000);
}
