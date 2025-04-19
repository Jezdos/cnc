using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Services
{
    public class AppInitializeService(FMqttClientManagement clientManagement) : IHostedService
    {

        FMqttClientManagement clientManagement = clientManagement;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await clientManagement.InitAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
