namespace Core.Utils
{
    public class WindowsProviderService
    {

        private readonly IServiceProvider _serviceProvider;

        public WindowsProviderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateServiceScope()
        {
            return _serviceProvider.CreateScope();
        }

        public T? GetService<T>() where T : class
        {
            if (_serviceProvider == null) return null;
            return _serviceProvider.GetService(typeof(T)) as T;
        }

        public T GetRequiredService<T>() where T : class
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("The service is not registered.");
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
