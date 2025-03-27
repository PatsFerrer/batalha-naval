using NavalBattle.Domain.Models;

namespace NavalBattle.Domain.Services.Interfaces
{
    public interface IMessageService
    {
        Task SendMessageAsync(Message message);
        Task<Message> ReceiveMessageAsync();
        Task StartListeningAsync(Func<Message, Task> messageHandler);
        Task StopListeningAsync();
    }
} 