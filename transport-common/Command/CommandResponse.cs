namespace transport_common.Command
{
    public class CommandResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }


        public CommandResponse(int StatusCode = 200, string Message = "", object? Data = null)
        {
            this.StatusCode = StatusCode;
            this.Message = Message;
            this.Data = Data;
        }
    }
}
