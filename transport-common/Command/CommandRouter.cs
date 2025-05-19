using System.Text.Json;

namespace transport_common.Command
{
    public abstract class CommandRouter : IConnectLifeCycle
    {
        private readonly Dictionary<string, Func<Dictionary<string, object>, Task<CommandResponse>>> _handlers = new(StringComparer.OrdinalIgnoreCase);

        // 注册处理方法
        public void RegisterHandler(string methodName, Func<Dictionary<string, object>, Task<CommandResponse>> handler) => _handlers[methodName] = handler;

        // 执行路由

        public async Task<CommandResponse> ExecuteCommandAsync(string payload)
        {
            string error_message = "";
            try
            {
                CommandRequest? command = JsonSerializer.Deserialize<CommandRequest>(payload, new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true });
                if (command is not null) return await ExecuteCommandAsync(command);
            }
            catch (Exception ex)
            {
                error_message = ex.Message;
            }

            return new CommandResponse
            {
                StatusCode = 404,
                Message = error_message,
            };
        }

        public async Task<CommandResponse> ExecuteCommandAsync(CommandRequest command)
        {
            if (!_handlers.TryGetValue(command.Method, out var handler))
                return new CommandResponse
                {
                    StatusCode = 404,
                    Message = $"Method {command.Method} not found"
                };

            try
            {
                var result = await handler(command.Params ?? new());
                return result;
            }
            catch (Exception ex)
            {
                return new CommandResponse
                {
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }


        protected abstract void RegisterCommandRouter();
    }
}
