using log4net;

namespace Core.Handler
{
    public delegate Task<bool> FormSubmitEventHandler<T>(T entity) where T : class;

    public delegate void SuccessEventHandler();

    public static class FormSubmitHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(FormSubmitHelper));

        public static Task<bool> SafeInvoke<T>(this FormSubmitEventHandler<T>? handler, T entity) where T : class
        {
            if (handler == null) return Task.FromResult(false); ;

            try
            {
                return handler.Invoke(entity);
            }
            catch (Exception ex)
            {
                // 可以做日志记录
                logger.ErrorFormat("FormSubmit 处理异常 : {0}", ex.Message);
                return Task.FromResult(false);
            }
        }
    }
}



