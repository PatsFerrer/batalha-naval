using NavalBattle.Core.Models;

namespace NavalBattle.Application.Interfaces
{
    public interface IMessageService
    {
        Task SendMessageAsync(Message message);
        Task<Message> ReceiveMessageAsync();
        Task StartListeningAsync(Func<Message, Task> messageHandler);
        Task StopListeningAsync();
    }
} 