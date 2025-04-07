using System.Security.Cryptography;
using System.Text;
using NavalBattle.Application.Interfaces;

namespace NavalBattle.Application.Services.Implementations
{
    public class CryptoService : ICryptoService
    {
        private readonly string _key;
        private readonly byte[] _salt;

        public CryptoService(string key, string salt)
        {
            _key = key;
            _salt = Encoding.UTF8.GetBytes(salt);
        }

        public string Encrypt(string content, string key)
        {
            using (Aes aes = Aes.Create())
            {
                // Gera um IV aleatório para cada mensagem
                aes.GenerateIV();
                
                // Deriva a chave usando PBKDF2
                var keyBytes = new Rfc2898DeriveBytes(key, _salt, 10000, HashAlgorithmName.SHA256).GetBytes(32);
                aes.Key = keyBytes;
                
                // Usa CBC com PKCS7 padding
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // Primeiro escreve o IV
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length);

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
                var encryptedBytes = Convert.FromBase64String(encryptedContent);
                
                // Lê o IV dos primeiros 16 bytes
                byte[] iv = new byte[16];
                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                aes.IV = iv;
                
                // Deriva a chave usando PBKDF2
                var keyBytes = new Rfc2898DeriveBytes(key, _salt, 10000, HashAlgorithmName.SHA256).GetBytes(32);
                aes.Key = keyBytes;
                
                // Usa CBC com PKCS7 padding
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes, 16, encryptedBytes.Length - 16))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
} 