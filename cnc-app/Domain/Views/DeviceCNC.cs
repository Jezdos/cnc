using APP.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APP.Domain.Views
{
    public partial class DeviceCNC
    {
        [JsonIgnore]
        public long? DeviceId { get; set; }

        [JsonIgnore]
        public string? Name { get; set; }

        [JsonIgnore]
        public DeviceModelEnum Model { get; set; } = DeviceModelEnum.AUTO;

        [JsonIgnore]
        public DeviceKindEnum Kind { get; set; }

        [JsonIgnore]
        public int? Interval { get; set; }

        public string? Host { get; set; }

        public int? Port { get; set; }

        public string? Path { get; set; }

        [JsonIgnore]
        public string? Params { get; set; }


        public DeviceCNC() { }

        public DeviceCNC(Device device)
        {
            DeviceId = device.DeviceId;
            Name = device.Name;
            Model = device.Model;
            Kind = device.Kind;
            Interval = device.Interval;
            Params = device.Params;

            this.Deserialize();
        }

        public string Serializer()
        {
            return JsonSerializer.Serialize(this);
        }

        private void Deserialize()
        {
            if (Params is not null)
            {
                DeviceCNC? entity = JsonSerializer.Deserialize<DeviceCNC>(Params);
                if (entity is not null)
                {
                    Host = entity.Host;
                    Port = entity.Port;
                    Path = entity.Path;
                }
            }

        }
    }
}
