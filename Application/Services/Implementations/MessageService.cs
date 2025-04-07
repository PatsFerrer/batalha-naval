using System.Text.Json;
using Azure.Messaging.ServiceBus;
using NavalBattle.Application.Interfaces;
using NavalBattle.Core.Models;

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

        public MessageService(
            string connectionString,
            string topicName,
            string subscriptionName,
            ICryptoService cryptoService)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _cryptoService = cryptoService;
            _cryptoBreaker = new CryptoBreaker();
            _origin = subscriptionName; // Usa o nome da subscription como origem

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
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
            // Comentando a criptografia temporariamente para testes
            // var encryptedContent = _cryptoService.Encrypt(message.conteudo, message.correlationId);

            // Cria uma nova mensagem com o conteúdo criptografado
            var messageToSend = new Message
            {
                correlationId = message.correlationId,
                origem = message.origem,
                evento = message.evento,
                // conteudo = encryptedContent
                conteudo = message.conteudo // Usando conteúdo sem criptografia
            };

            var messageContent = JsonSerializer.Serialize(messageToSend, _jsonOptions);
            var serviceBusMessage = new ServiceBusMessage(messageContent)
            {
                CorrelationId = message.correlationId,
                ApplicationProperties = { { "Origin", message.origem } }
            };

            await _sender.SendMessageAsync(serviceBusMessage);
        }

        public async Task<Message> ReceiveMessageAsync()
        {
            var receivedMessage = await _receiver.ReceiveMessageAsync();
            if (receivedMessage == null) return null;

            var message = JsonSerializer.Deserialize<Message>(receivedMessage.Body.ToString(), _jsonOptions);
            
            // Descriptografa apenas o conteúdo
            message.conteudo = _cryptoService.Decrypt(message.conteudo, message.correlationId);

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
                    message = JsonSerializer.Deserialize<Message>(args.Message.Body.ToString(), _jsonOptions);
                    
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
                    
                    message.conteudo = _cryptoService.Decrypt(message.conteudo, message.correlationId);
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("Mensagem recebida (conteúdo criptografado)");
                }
                catch
                {
                    // Se falhar a descriptografia, tenta ler a mensagem direta
                    message = JsonSerializer.Deserialize<Message>(args.Message.Body.ToString(), _jsonOptions);
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