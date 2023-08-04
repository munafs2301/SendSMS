using SharedResource.Events;

namespace SmsService.Repositories
{
    public interface IMessageRepo
    {
        void AddMessage(IMessage message);
        void UpdateMessage(IMessage receivedMessage);
    }
}
