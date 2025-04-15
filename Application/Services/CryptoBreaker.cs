using System.Security.Cryptography;
using System.Text;
using NavalBattle.Core.Models;
using NavalBattle.Core.Models.MessageContent;

namespace NavalBattle.Application.Services
{
    public class CryptoBreaker
    {
        private readonly List<(string Encrypted, string Decrypted)> _messagePairs = new();
        private List<string> _possibleKeys = new();
        private readonly Random _random = new();
        private Position _enemyShipPosition;
        private List<Position> _enemyShipPositions;
        private string _enemyShipName;

        public void AddMessagePair(string encrypted, string decrypted)
        {
            _messagePairs.Add((encrypted, decrypted));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[CRYPTO BREAKER] Nova mensagem interceptada!");
            Console.WriteLine($"Criptografada: {encrypted}");
            Console.WriteLine($"Descriptografada: {decrypted}");
            Console.ResetColor();
        }

        public void AnalyzeMessages()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[CRYPTO BREAKER] Analisando mensagens...");
            
            foreach (var (encrypted, decrypted) in _messagePairs)
            {
                var encryptedBytes = Convert.FromBase64String(encrypted);
                var decryptedBytes = Encoding.UTF8.GetBytes(decrypted);
                
                // Analisa o padrão de criptografia
                Console.WriteLine($"\nTamanho criptografado: {encryptedBytes.Length} bytes");
                Console.WriteLine($"Tamanho original: {decryptedBytes.Length} bytes");
                
                // Verifica se é múltiplo de 16 (AES)
                if (encryptedBytes.Length % 16 == 0)
                {
                    Console.WriteLine("Padrão AES detectado (tamanho múltiplo de 16 bytes)");
                }
                
                // Analisa o padrão de bytes
                var uniqueBytes = encryptedBytes.Distinct().Count();
                Console.WriteLine($"Bytes únicos: {uniqueBytes} de {encryptedBytes.Length}");
                
                // Verifica se há padrões repetitivos
                var patterns = FindRepeatingPatterns(encryptedBytes);
                if (patterns.Any())
                {
                    Console.WriteLine("Padrões repetitivos encontrados:");
                    foreach (var pattern in patterns)
                    {
                        Console.WriteLine($"- Padrão de {pattern.Pattern.Length} bytes encontrado {pattern.Count} vezes");
                    }
                }

                // Analisa o IV (primeiros 16 bytes)
                var iv = encryptedBytes.Take(16).ToArray();
                Console.WriteLine($"IV (primeiros 16 bytes): {BitConverter.ToString(iv)}");
            }
            
            Console.ResetColor();
        }

        public void GeneratePossibleKeys()
        {
            _possibleKeys.Clear();
            
            // Padrões comuns de chaves criptográficas
            var commonPatterns = new[]
            {
                // Padrões com caracteres especiais
                "!@#$%^&*()",
                "!@#$%^&*()_+",
                "!@#$%^&*()_+-=",
                
                // Padrões com números
                "1234567890",
                "9876543210",
                "0123456789",
                
                // Padrões com letras e números
                "qwerty123",
                "asdfgh123",
                "zxcvbn123",
                
                // Padrões com caracteres especiais e números
                "!@#$%123",
                "!@#$%456",
                "!@#$%789",
                
                // Padrões com letras maiúsculas e minúsculas
                "QwErTy123",
                "AsDfGh123",
                "ZxCvBn123",
                
                // Padrões com caracteres especiais, letras e números
                "!@#$%QwErTy123",
                "!@#$%AsDfGh123",
                "!@#$%ZxCvBn123"
            };
            
            // Gera variações dos padrões
            foreach (var pattern in commonPatterns)
            {
                // Adiciona o padrão original
                _possibleKeys.Add(pattern);
                
                // Adiciona variações com diferentes combinações de maiúsculas/minúsculas
                _possibleKeys.Add(pattern.ToUpper());
                _possibleKeys.Add(pattern.ToLower());
                
                // Adiciona variações com diferentes posições de caracteres especiais
                var chars = pattern.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (char.IsLetter(chars[i]))
                    {
                        var temp = (char[])chars.Clone();
                        temp[i] = char.IsUpper(temp[i]) ? char.ToLower(temp[i]) : char.ToUpper(temp[i]);
                        _possibleKeys.Add(new string(temp));
                    }
                }
            }
            
            // Remove duplicatas
            _possibleKeys = _possibleKeys.Distinct().ToList();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[CRYPTO BREAKER] Geradas {_possibleKeys.Count} possíveis chaves baseadas em padrões comuns de criptografia");
            Console.ResetColor();
        }

        public async Task TryBreakEncryption(string encryptedMessage)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[CRYPTO BREAKER] Tentando quebrar criptografia...");
            
            foreach (var key in _possibleKeys)
            {
                try
                {
                    var decrypted = DecryptWithKey(encryptedMessage, key);
                    if (IsValidJson(decrypted))
                    {
                        Console.WriteLine($"\n[CRYPTO BREAKER] CHAVE ENCONTRADA: {key}");
                        Console.WriteLine($"Mensagem descriptografada: {decrypted}");
                        
                        // Extrai as posições do navio inimigo
                        ExtractEnemyShipPositions(decrypted);
                        return;
                    }
                }
                catch
                {
                    // Ignora erros de descriptografia
                }
            }
            
            Console.WriteLine("[CRYPTO BREAKER] Não foi possível quebrar a criptografia com as chaves atuais");
            Console.ResetColor();
        }

        private string DecryptWithKey(string encrypted, string key)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encrypted);
                
                // Tenta diferentes modos de operação AES
                var modes = new[] { CipherMode.CBC, CipherMode.ECB, CipherMode.CFB };
                var paddings = new[] { PaddingMode.PKCS7, PaddingMode.Zeros, PaddingMode.ANSIX923 };
                
                foreach (var mode in modes)
                {
                    foreach (var padding in paddings)
                    {
                        using (Aes aes = Aes.Create())
                        {
                            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                            aes.Key = keyBytes;
                            
                            // Se for CBC, usa os primeiros 16 bytes como IV
                            if (mode == CipherMode.CBC && encryptedBytes.Length >= 16)
                            {
                                aes.IV = encryptedBytes.Take(16).ToArray();
                                aes.Mode = mode;
                                aes.Padding = padding;
                                
                                try
                                {
                                    using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes.Skip(16).ToArray()))
                                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                    {
                                        var decrypted = srDecrypt.ReadToEnd();
                                        if (IsValidJson(decrypted))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"\n[CRYPTO BREAKER] Chave encontrada: {key}");
                                            Console.WriteLine($"Modo: {mode}, Padding: {padding}");
                                            Console.ResetColor();
                                            return decrypted;
                                        }
                                    }
                                }
                                catch
                                {
                                    // Continua tentando outros modos
                                }
                            }
                            else
                            {
                                aes.IV = new byte[16]; // IV padrão para outros modos
                                aes.Mode = mode;
                                aes.Padding = padding;
                                
                                try
                                {
                                    using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                    {
                                        var decrypted = srDecrypt.ReadToEnd();
                                        if (IsValidJson(decrypted))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"\n[CRYPTO BREAKER] Chave encontrada: {key}");
                                            Console.WriteLine($"Modo: {mode}, Padding: {padding}");
                                            Console.ResetColor();
                                            return decrypted;
                                        }
                                    }
                                }
                                catch
                                {
                                    // Continua tentando outros modos
                                }
                            }
                        }
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool IsValidJson(string text)
        {
            try
            {
                var obj = System.Text.Json.JsonSerializer.Deserialize<object>(text);
                return obj != null;
            }
            catch
            {
                return false;
            }
        }

        private List<(byte[] Pattern, int Count)> FindRepeatingPatterns(byte[] data)
        {
            var patterns = new List<(byte[] Pattern, int Count)>();
            
            // Procura por padrões de 2 a 8 bytes
            for (int patternLength = 2; patternLength <= 8; patternLength++)
            {
                var patternCounts = new Dictionary<string, int>();
                
                for (int i = 0; i <= data.Length - patternLength; i++)
                {
                    var pattern = data.Skip(i).Take(patternLength).ToArray();
                    var patternKey = Convert.ToBase64String(pattern);
                    
                    if (patternCounts.ContainsKey(patternKey))
                        patternCounts[patternKey]++;
                    else
                        patternCounts[patternKey] = 1;
                }
                
                // Adiciona padrões que aparecem mais de uma vez
                foreach (var kvp in patternCounts.Where(x => x.Value > 1))
                {
                    patterns.Add((Convert.FromBase64String(kvp.Key), kvp.Value));
                }
            }
            
            return patterns;
        }

        private void ExtractEnemyShipPositions(string decryptedJson)
        {
            try
            {
                var message = System.Text.Json.JsonSerializer.Deserialize<Message>(decryptedJson);
                if (message != null && message.conteudo != null)
                {
                    var content = System.Text.Json.JsonSerializer.Deserialize<RegistroNavioContent>(message.conteudo);
                    if (content != null)
                    {
                        _enemyShipName = content.nomeNavio;
                        _enemyShipPosition = new Position(content.posicaoCentral.X, content.posicaoCentral.Y);
                        
                        // Calcula as posições do navio baseado na posição central e orientação
                        _enemyShipPositions = new List<Position>();
                        var isHorizontal = content.orientacao.ToLower() == "horizontal";
                        
                        // O navio tem 5 posições, com a central sendo a terceira
                        for (int i = -2; i <= 2; i++)
                        {
                            int x = isHorizontal ? content.posicaoCentral.X + i : content.posicaoCentral.X;
                            int y = isHorizontal ? content.posicaoCentral.Y : content.posicaoCentral.Y + i;
                            _enemyShipPositions.Add(new Position(x, y));
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n[CRYPTO BREAKER] Posições do navio inimigo ({_enemyShipName}) encontradas!");
                        Console.WriteLine($"Posição central: X={_enemyShipPosition.PosX}, Y={_enemyShipPosition.PosY}");
                        Console.WriteLine($"Orientação: {content.orientacao}");
                        Console.WriteLine($"Total de posições: {_enemyShipPositions.Count}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao extrair posições: {ex.Message}");
                Console.ResetColor();
            }
        }

        public Position GetEnemyShipPosition()
        {
            return _enemyShipPosition;
        }

        public List<Position> GetEnemyShipPositions()
        {
            return _enemyShipPositions;
        }

        public string GetEnemyShipName()
        {
            return _enemyShipName;
        }
    }
} 