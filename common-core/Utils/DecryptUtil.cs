using log4net;
using System.Security.Cryptography;

namespace Core.Utils
{
    public class AesCryptographer
    {
        private static readonly byte[] KEY = [0xD6, 0xE0, 0x8A, 0x9A, 0x6F, 0x03, 0x83, 0xF6, 0x68, 0x17, 0x59, 0xF2, 0x30, 0x0A, 0x6B, 0x14, 0x20, 0x53, 0x97, 0x95, 0xCE, 0x98, 0xA4, 0x56, 0xBD, 0x7D, 0x8F, 0x4F, 0x4F, 0x18, 0xB4, 0x38];

        public static string EncryptString(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KEY;
                aesAlg.GenerateIV(); // 动态生成 IV
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // 将 IV 写入密文头部
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KEY;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 从密文中提取 IV
                byte[] iv = new byte[aesAlg.IV.Length];
                Array.Copy(buffer, iv, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public class BCryptCryptographer
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(BCryptCryptographer));

        public static string HashPassword(string pro, int workFactor = 10)
        {
            try
            {
                string salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
                return BCrypt.Net.BCrypt.HashPassword(pro, salt);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("BCryptCryptographer HashPassword Error: {0}", ex.Message);
                return string.Empty;
            }
        }

        public static bool Verify(string? str, string? encryptStr)
        {
            try
            {
                if (str == null || encryptStr == null) return false;
                return BCrypt.Net.BCrypt.Verify(str, encryptStr);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("BCryptCryptographer Verify Error: {0}", ex.Message);
                return false;
            }
        }
    }
}
