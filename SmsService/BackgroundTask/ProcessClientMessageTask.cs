using SharedResource.Events;
using SharedResource.Logger;
using SharedResource.HttpClient;
using SharedResource.Events.Constants;
using System.Text.Json;
using SmsService.Constants;
using SmsService.Entities;
using SmsService.Repositories;

namespace SmsService.BackgroundTask
{
    public class ProcessClientMessageTask
    {
        private readonly ISmsLogger _logger;
        private readonly IMessageRepo _messageRepo;
        private readonly IEventBus _eventBus;
        private readonly ISendSMSHttpClient _sendSMSHttpClient;

        public ProcessClientMessageTask(IEventBus messageBus, ISendSMSHttpClient sendSMSHttpClient, IMessageRepo messageRepo, ISmsLogger logger)
        {
            _eventBus = messageBus;
            _sendSMSHttpClient = sendSMSHttpClient;
            _messageRepo = messageRepo;
            _logger = logger;
        }

        public async Task Run()
        {
            // Stage one
            var message = _eventBus.ConsumeCommand(QueueConstants.SmsService);
            if (message == null) return;

            var receivedMessage = JsonSerializer.Deserialize<Message>(message);

            _messageRepo.AddMessage(receivedMessage);

            // Stage two
            var response = await _sendSMSHttpClient.PostToExternalApi<IMessage>(receivedMessage);
            if (!response.IsSuccessStatusCode)
            {
                UpdateMessageStatus(receivedMessage, MessageStatus.Errored);
                _logger.Error("--> Message not sent to Third Party");
                return;
                /// Add Assumption to readme that a cron job will run periodically to pick up failed messages
                /// //Assume there is background service to run the ProcessClientMessageTask throughout the application lifescop because it has to listen for SmsCommand 
                /// Assume We will implement a cache mechanism for persistence purpose. Will be implemented in IMessageRepo
            }

            UpdateMessageStatus(receivedMessage, MessageStatus.SentExternally);
            _logger.Info("Message sent successfully");

            // Stage three
            try
            {
                var smsSent = new SmsSentEvent { EventType = EventConstants.SmsSentEvent };
                _eventBus.PublishEvent(smsSent);
            }
            catch (Exception ex)
            {
                UpdateMessageStatus(receivedMessage, MessageStatus.Errored);
                _logger.Error($"An error occured while publishing the Event: {ex.Message}");
                return;
            }

            UpdateMessageStatus(receivedMessage, MessageStatus.Published);
            _logger.Info("--> SmsSent Event Published successfully successfully");
            return;
        }

        private void UpdateMessageStatus(Message receivedMessage, MessageStatus sentExternally)
        {
            receivedMessage.Status = MessageStatus.SentExternally;
            _messageRepo.UpdateMessage(receivedMessage);
        }
    }

}
