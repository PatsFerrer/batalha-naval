using System.Security.Cryptography;
using System.Text;
using NavalBattle.Application.Interfaces;

namespace NavalBattle.Application.Services.Implementations
{
    public class CryptoService : ICryptoService
    {
        private readonly string _key;

        public CryptoService(string key)
        {
            _key = key;
        }

        private byte[] GetKeyBytes(string key)
        {
            // Converte a string hexadecimal para bytes
            byte[] keyBytes = new byte[key.Length / 2];
            for (int i = 0; i < keyBytes.Length; i++)
            {
                keyBytes[i] = Convert.ToByte(key.Substring(i * 2, 2), 16);
            }
            return keyBytes;
        }

        public string Encrypt(string content, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKeyBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new((Stream)cryptoStream))
                {
                    streamWriter.Write(content);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        public string Decrypt(string encryptedContent, string key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(encryptedContent);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = GetKeyBytes(key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using MemoryStream memoryStream = new(buffer);
                    using CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
                    using (StreamReader streamReader = new((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return encryptedContent;
            }
        }
    }
} 