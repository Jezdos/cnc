using Microsoft.Extensions.Hosting;

namespace APP.Services
{
    public class AppInitializeService(FMqttClientManagement clientManagement) : IHostedService
    {

        private readonly FMqttClientManagement clientManagement = clientManagement;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ActivityHSLCommunication();
            await clientManagement.InitAsync();
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
