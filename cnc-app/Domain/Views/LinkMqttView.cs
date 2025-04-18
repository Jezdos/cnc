using Core.Enums;

namespace APP.Domain.Views
{
    public class LinkMqttView(LinkMqtt entity) : LinkMqtt(entity)
    {
        public BaseStatusEnum Status { get; set; } = BaseStatusEnum.EXCEPTION;
    }
}
