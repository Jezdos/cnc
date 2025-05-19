using Microsoft.Extensions.Hosting;

namespace APP.Services
{
    public class AppInitializeService(FMqttClientManagement mqttClientManagement, DeviceClientManagement deviceClientManagement, AdaptorManagement adaptorManagement) : IHostedService
    {

        private readonly FMqttClientManagement mqttClientManagement = mqttClientManagement;
        private readonly DeviceClientManagement deviceClientManagement = deviceClientManagement;
        private readonly AdaptorManagement adaptorManagement = adaptorManagement;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ActivityHSLCommunication();
            await mqttClientManagement.InitAsync();
            await deviceClientManagement.InitAsync();
            await adaptorManagement.InitAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        private Task ActivityHSLCommunication()
        {
            if (!HslCommunication.Authorization.SetAuthorizationCode("ca8f4334-5b43-4e86-b488-725007c4eb44"))
            {
                throw new Exception("HSLCommunication Activity Failed!");
            }
            return Task.CompletedTask;
        }

    }
}
