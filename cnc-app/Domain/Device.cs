using APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APP.Domain
{
    [Table("device")]
    public class Device
    {
        [Key]
        [Column("device_id")]
        public long? DeviceId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("model")]
        public DeviceModelEnum Model { get; set; } = DeviceModelEnum.AUTO;

        [Column("kind")]
        public DeviceKindEnum Kind { get; set; }

        [Column("interval")]
        public int? Interval { get; set; }

        [Column("params")]
        public string? Params { get; set; }

        public Device() { }

        public Device(Device entity)
        {
            DeviceId = entity.DeviceId;
            Name = entity.Name;
            Model = entity.Model;
            Kind = entity.Kind;
            Interval = entity.Interval;
            Params = entity.Params;
        }
    }
}
