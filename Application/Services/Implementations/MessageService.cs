using System.Text.Json;
using Azure.Messaging.ServiceBus;
using NavalBattle.Application.Interfaces;
using NavalBattle.Core.Models;
using NavalBattle.Core.Helpers;

namespace NavalBattle.Application.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly ICryptoService _cryptoService;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;
        private readonly ServiceBusReceiver _receiver;
        private readonly JsonSerializerOptions _jsonOptions;
        private Func<Message, Task> _messageHandler;
        private readonly CryptoBreaker _cryptoBreaker;
        private readonly string _origin;
        private readonly string _cryptoKey;

        public MessageService(
            string connectionString,
            string topicName,
            string subscriptionName,
            ICryptoService cryptoService,
            string cryptoKey)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _cryptoService = cryptoService;
            _cryptoKey = cryptoKey;
            _cryptoBreaker = new CryptoBreaker();
            _origin = subscriptionName; // Usa o nome da subscription como origem

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(_topicName);
            _processor = _client.CreateProcessor(_topicName, _subscriptionName);
            _receiver = _client.CreateReceiver(_topicName, _subscriptionName);

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
        }

        public async Task SendMessageAsync(Message message)
        {
            // Cria uma nova mensagem com o conteúdo criptografado
            var encryptedContent = _cryptoService.Encrypt(message.conteudo, _cryptoKey);

            var messageToSend = new Message
            {
                correlationId = message.correlationId,
                origem = message.origem,
                evento = message.evento,
                conteudo = encryptedContent
            };

            var messageContent = messageToSend.Serialize();
            var serviceBusMessage = new ServiceBusMessage(messageContent)
            {
                CorrelationId = message.correlationId,
                ApplicationProperties = { { "Origin", message.origem } }
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nEnviando mensagem:");
            Console.WriteLine($"Evento: {message.evento}");
            Console.WriteLine($"Conteúdo original: {message.conteudo}");
            Console.WriteLine($"Conteúdo criptografado: {encryptedContent}");
            Console.ResetColor();

            await _sender.SendMessageAsync(serviceBusMessage);
        }

        public async Task<Message> ReceiveMessageAsync()
        {
            var receivedMessage = await _receiver.ReceiveMessageAsync();
            if (receivedMessage == null) return null;

            var message = receivedMessage.Body.ToString().Deserialize<Message>();
            
            // Descriptografa apenas o conteúdo
            message.conteudo = _cryptoService.Decrypt(message.conteudo, _cryptoKey);

            return message;
        }

        public async Task StartListeningAsync(Func<Message, Task> messageHandler)
        {
            _messageHandler = messageHandler;
            await _processor.StartProcessingAsync();
        }

        public async Task StopListeningAsync()
        {
            await _processor.StopProcessingAsync();
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                Message message;
                try
                {
                    // Tenta primeiro descriptografar apenas o conteúdo
                    message = args.Message.Body.ToString().Deserialize<Message>();
                    
                    // Se a mensagem é para outro navio, tenta quebrar a criptografia
                    if (!string.IsNullOrEmpty(message.navioDestino) && message.navioDestino != _origin)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[CRYPTO BREAKER] Interceptando mensagem para outro navio!");
                        Console.WriteLine($"Navio destino: {message.navioDestino}");
                        Console.WriteLine($"Conteúdo criptografado: {message.conteudo}");
                        Console.ResetColor();
                        
                        // Gera possíveis chaves
                        _cryptoBreaker.GeneratePossibleKeys();
                        
                        // Tenta quebrar a criptografia
                        await _cryptoBreaker.TryBreakEncryption(message.conteudo);
                    }
                    
                    message.conteudo = _cryptoService.Decrypt(message.conteudo, _cryptoKey);
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("Mensagem recebida (conteúdo criptografado)");
                }
                catch
                {
                    // Se falhar a descriptografia, tenta ler a mensagem direta
                    message = args.Message.Body.ToString().Deserialize<Message>();
                    Console.WriteLine("Mensagem recebida (não criptografada)");
                    Console.WriteLine("-------------------------------------");
                }

                Console.WriteLine($"Origem: {message?.origem ?? "Desconhecida"}");
                Console.WriteLine($"Evento: {message?.evento ?? "Desconhecido"}");
                Console.WriteLine($"Conteúdo: {message?.conteudo ?? "Vazio"}");
                Console.WriteLine($"CorrelationId: {message?.correlationId ?? "Não informado"}");
                Console.WriteLine($"NavioDestino: {message?.navioDestino ?? "Não informado"}");

                if (message != null)
                {
                    await _messageHandler(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
            }
            finally
            {
                await args.CompleteMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            // Log do erro
            Console.WriteLine($"Erro no processamento: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.DisposeAsync();
            await _sender.DisposeAsync();
            await _receiver.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
} 