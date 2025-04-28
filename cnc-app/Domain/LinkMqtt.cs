using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APP.Domain
{
    [Table("link_mqtt")]
    public class LinkMqtt
    {
        [Key]
        [Column("link_id")]
        public long? LinkId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("host")]
        public string? Host { get; set; }

        [Column("port")]
        public int? Port { get; set; }

        [Column("client_id")]
        public string? ClientId { get; set; }

        [Column("keep_alive")]
        public int? KeepAlive { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("model")]
        public LinkModelEnum Model { get; set; }

        public LinkMqtt() { }

        public LinkMqtt(LinkMqtt entity)
        {
            LinkId = entity.LinkId;
            Name = entity.Name;
            Host = entity.Host;
            Port = entity.Port;
            ClientId = entity.ClientId;
            KeepAlive = entity.KeepAlive;
            Username = entity.Username;
            Password = entity.Password;
            Model = entity.Model;
        }
    }

    public enum LinkModelEnum
    {
        AUTO, SUSPEND
    }
}
