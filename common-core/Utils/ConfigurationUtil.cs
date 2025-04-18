using log4net;
using Microsoft.Extensions.Configuration;

namespace Core.Utils
{
    public class ConfigurationUtil
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(ConfigurationUtil));

        private const string _appSettings = "./appdata/appsettings.json";

        private readonly IConfigurationRoot _configuration;

        // 构造函数：支持依赖注入
        public ConfigurationUtil(IConfigurationRoot configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // 静态构造函数：提供默认实例
        static ConfigurationUtil()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_appSettings, optional: true, reloadOnChange: true);

            Default = new ConfigurationUtil(builder.Build());
        }

        // 默认实例
        public static ConfigurationUtil Default { get; }

        /// <summary>
        /// 读取配置项
        /// </summary>
        /// <param name="key">配置项键</param>
        /// <param name="defaultValue">默认值（可选）</param>
        /// <param name="decode">是否解密（可选）</param>
        /// <returns>配置项值</returns>
        public string ReadConfiguration(string key, string defaultValue = "", bool decode = false)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            string? value = _configuration[key];

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (decode)
            {
                try
                {
                    return AesCryptographer.DecryptString(value);
                }
                catch (Exception ex)
                {
                    // 记录日志或处理异常
                    logger.Error($"配置项解密失败: {ex.Message}");
                    return defaultValue;
                }
            }

            return value;
        }

        /// <summary>
        /// 读取配置项并映射到强类型对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="key">配置项键</param>
        /// <returns>强类型对象</returns>
        public T GetSection<T>(string key) where T : class, new()
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return _configuration.GetSection(key).Get<T>() ?? new T();
        }
    }
}
