using System.Text.Json;
using Azure.Messaging.ServiceBus;
using NavalBattle.Domain.Models;
using NavalBattle.Domain.Services.Interfaces;

namespace NavalBattle.Domain.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;
        private readonly ServiceBusReceiver _receiver;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly ICryptoService _cryptoService;
        private Func<Message, Task> _messageHandler;

        public MessageService(
            string connectionString,
            string topicName,
            string subscriptionName,
            ICryptoService cryptoService)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _cryptoService = cryptoService;

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(_topicName);
            _processor = _client.CreateProcessor(_topicName, _subscriptionName);
            _receiver = _client.CreateReceiver(_topicName, _subscriptionName);

            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
        }

        public async Task SendMessageAsync(Message message)
        {
            var messageContent = JsonSerializer.Serialize(message);
            var encryptedContent = _cryptoService.Encrypt(messageContent, message.correlationId);

            var serviceBusMessage = new ServiceBusMessage(encryptedContent)
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

            var decryptedContent = _cryptoService.Decrypt(
                receivedMessage.Body.ToString(),
                receivedMessage.CorrelationId);

            return JsonSerializer.Deserialize<Message>(decryptedContent);
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
                    // Tenta primeiro descriptografar
                    var decryptedContent = _cryptoService.Decrypt(
                        args.Message.Body.ToString(),
                        args.Message.CorrelationId);
                    message = JsonSerializer.Deserialize<Message>(decryptedContent);
                    Console.WriteLine("Mensagem recebida (criptografada)");
                }
                catch
                {
                    // Se falhar a descriptografia, tenta ler a mensagem direta
                    message = JsonSerializer.Deserialize<Message>(args.Message.Body.ToString());
                    Console.WriteLine("Mensagem recebida (não criptografada)");
                }

                Console.WriteLine($"Origem: {message?.origem ?? "Desconhecida"}");
                Console.WriteLine($"Evento: {message?.evento ?? "Desconhecido"}");
                Console.WriteLine($"Conteúdo: {message?.conteudo ?? "Vazio"}");
                Console.WriteLine($"CorrelationId: {message?.correlationId ?? "Não informado"}");

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