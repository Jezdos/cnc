using System.Text.Json.Serialization;

namespace transport_common.Command
{
    public class CommandRequest
    {
        [JsonRequired]
        public string? Method { get; set; }    // 方法名称

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Params { get; set; } // 参数键值对

        public DateTime Timestamp { get; set; }

        public CommandRequest()
        {
            Timestamp = DateTime.Now;
        }

        public CommandRequest(string method, Dictionary<string, object> @params, string mid)
        {
            Method = method;
            Params = @params;
            Timestamp = DateTime.Now;
        }
    }
}
