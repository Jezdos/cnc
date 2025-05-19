using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APP.Domain
{
    [Table("adaptor")]
    public class Adaptor
    {
        [Key]
        [Column("adaptor_id")]
        public long? AdaptorId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("device_id")]
        public long? DeviceId { get; set; }

        [Column("link_id")]
        public long? LinkId { get; set; }

        [Column("topic_telemetry")]
        public string? TopicTelemetry { get; set; } = "v1/devices/me/telemetry";

        [Column("topic_rpc")]
        public string? TopicRpc { get; set; } = "v1/devices/me/rpc/request/+";

        public Adaptor() { }

        public Adaptor(Adaptor entity)
        {
            AdaptorId = entity.AdaptorId;
            Name = entity.Name;
            DeviceId = entity.DeviceId;
            LinkId = entity.LinkId;
            TopicTelemetry = entity.TopicTelemetry;
            TopicRpc = entity.TopicRpc;
        }
    }
}
