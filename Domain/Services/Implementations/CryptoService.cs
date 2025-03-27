using System.Security.Cryptography;
using System.Text;
using NavalBattle.Domain.Services.Interfaces;

namespace NavalBattle.Domain.Services.Implementations
{
    public class CryptoService : ICryptoService
    {
        private readonly string _key;

        public CryptoService(string key)
        {
            _key = key;
        }

        public string Encrypt(string content, string key)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // IV fixo para simplicidade

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(content);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string encryptedContent, string key)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // IV fixo para simplicidade

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedContent)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
} 