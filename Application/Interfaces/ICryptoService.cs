namespace NavalBattle.Application.Interfaces
{
    public interface ICryptoService
    {
        string Encrypt(string content, string key);
        string Decrypt(string encryptedContent, string key);
    }
} 