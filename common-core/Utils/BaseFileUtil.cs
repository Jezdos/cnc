using log4net;

namespace Core.Utils
{
    public class BaseFileUtil
    {
        private static readonly string BaseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");

        private const string DefaultImagePath = "pack://application:,,,/common-core;component/Assets/default.png";

        private static readonly SnowflakeIdWorker IdWorker = SnowflakeIdWorker.Singleton;

        private static readonly ILog logger = LogManager.GetLogger(nameof(BaseFileUtil));

        static BaseFileUtil()
        {
            Directory.CreateDirectory(BaseFilePath);
        }

        public static string? UpdateFile(string? sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                return null;

            try
            {
                string extension = Path.GetExtension(sourceFilePath);
                string destinationFilePath = GenNextFileName(extension);

                File.Copy(sourceFilePath, destinationFilePath, true);

                return Path.GetFileName(destinationFilePath);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("BaseFileUtil UpdateFile Error: {0}", ex.Message);
            }

            return null;
        }


        public static string? GetOriFilePath(string? fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string oriFileAddress = Path.Combine(BaseFilePath, fileName);
                if (File.Exists(oriFileAddress)) return oriFileAddress;
            };
            return DefaultImagePath;
        }

        private static string GenNextFileName(string extension)
        {
            return Path.Combine(BaseFilePath, IdWorker.nextId().ToString() + extension);
        }
    }
}
