using Moq;
using SharedResource.Events;
using SharedResource.Events.Constants;
using SharedResource.HttpClient;
using SharedResource.Logger;
using SmsService.BackgroundTask;
using SmsService.Constants;
using SmsService.Entities;
using SmsService.Repositories;
using System.Text.Json;

namespace SmsService.Tests.BackgroundTask
{
    public class ProcessClientMessageTaskTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<ISendSMSHttpClient> _sendSMSHttpClient;
        private Mock<IMessageRepo> _messageRepo;
        private Mock<ISmsLogger> _logger;

        [SetUp]
        public void Setup()
        {
            _eventBus = new Mock<IEventBus>();
            _sendSMSHttpClient = new Mock<ISendSMSHttpClient>();
            _messageRepo = new Mock<IMessageRepo>();
            _logger = new Mock<ISmsLogger>();
        }


        [Test]
        public async Task Should_Send_Message_And_Publish_Event()
        {
            // Arrange
            var receivedMessage = new Message { PhoneNumber = "09036417661", SmsText = "Thanks for subscribing", Status = MessageStatus.Pending };
            var jsonString = JsonSerializer.Serialize<Message>(receivedMessage);
            var response = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };

            _eventBus.Setup(e => e.ConsumeCommand(QueueConstants.SmsService)).Returns(jsonString);
            _sendSMSHttpClient.Setup(e => e.PostToExternalApi<IMessage>(It.IsAny<Message>()))
                .ReturnsAsync(response);
            var processClientMessage = new ProcessClientMessageTask(_eventBus.Object, _sendSMSHttpClient.Object, _messageRepo.Object, _logger.Object);

            // Act
            await processClientMessage.Run();

            // Assert
            _messageRepo.Verify(repo => repo.AddMessage(It.IsAny<Message>()), Times.Once);
            _sendSMSHttpClient.Verify(httpClient => httpClient.PostToExternalApi<IMessage>(It.IsAny<Message>()), Times.Once);
            _eventBus.Verify(bus => bus.PublishEvent(It.Is<SmsSentEvent>(e => e.EventType == EventConstants.SmsSentEvent)), Times.Once);
            _messageRepo.Verify(repo => repo.UpdateMessage(It.IsAny<Message>()), Times.Exactly(2));
            _logger.Verify(logger => logger.Error(It.IsAny<string>()), Times.Never); 
            _logger.Verify(logger => logger.Info(It.IsAny<string>()), Times.Exactly(2)); 

        }


        [Test]
        public async Task Should_Cancel_Process_When_Message_Is_Null()
        {
            // Arrange
            string receivedMessage = null;
            _eventBus.Setup(e => e.ConsumeCommand(QueueConstants.SmsService)).Returns(receivedMessage);
            var processClientMessage = new ProcessClientMessageTask(_eventBus.Object, _sendSMSHttpClient.Object, _messageRepo.Object, _logger.Object);

            // Act
            await processClientMessage.Run();

            // Assert
            _messageRepo.Verify(repo => repo.AddMessage(It.IsAny<Message>()), Times.Never);
            _sendSMSHttpClient.Verify(httpClient => httpClient.PostToExternalApi<IMessage>(It.IsAny<Message>()), Times.Never);
            _eventBus.Verify(bus => bus.PublishEvent(It.IsAny<SmsSentEvent>()), Times.Never);
            _messageRepo.Verify(repo => repo.UpdateMessage(It.IsAny<Message>()), Times.Never);
            _logger.Verify(logger => logger.Error(It.IsAny<string>()), Times.Never);
            _logger.Verify(logger => logger.Info(It.IsAny<string>()), Times.Never);
        }


        [Test]
        public async Task Should_Cancel_Process_When_External_Api_Call_Fails()
        {
            // Arrange
            var response = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
            var receivedMessage = new Message { PhoneNumber = "09036417661", SmsText = "Thanks for subscribing", Status = MessageStatus.Pending };
            var jsonString = JsonSerializer.Serialize<Message>(receivedMessage);

            _eventBus.Setup(e => e.ConsumeCommand(QueueConstants.SmsService)).Returns(jsonString);
            _sendSMSHttpClient.Setup(e => e.PostToExternalApi<IMessage>(It.IsAny<Message>()))
                .ReturnsAsync(response);
            var processClientMessage = new ProcessClientMessageTask(_eventBus.Object, _sendSMSHttpClient.Object, _messageRepo.Object, _logger.Object);

            // Act
            await processClientMessage.Run();

            // Assert
            _messageRepo.Verify(repo => repo.AddMessage(It.IsAny<Message>()), Times.Once);
            _messageRepo.Verify(repo => repo.UpdateMessage(It.IsAny<Message>()), Times.Once);
            _sendSMSHttpClient.Verify(httpClient => httpClient.PostToExternalApi<IMessage>(It.IsAny<Message>()), Times.Once);
            _eventBus.Verify(bus => bus.PublishEvent(It.IsAny<SmsSentEvent>()), Times.Never);
            _logger.Verify(logger => logger.Error(It.IsAny<string>()), Times.Once);
            _logger.Verify(logger => logger.Info(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Should_Cancel_Process_When_PublishEvent_Throws_Exception()
        {
            // Arrange
            var response = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
            var receivedMessage = new Message { PhoneNumber = "09036417661", SmsText = "Thanks for subscribing", Status = MessageStatus.Pending };
            var jsonString = JsonSerializer.Serialize<Message>(receivedMessage);

            _sendSMSHttpClient.Setup(e => e.PostToExternalApi<IMessage>(It.IsAny<Message>()))
                .ReturnsAsync(response);
            _eventBus.Setup(e => e.ConsumeCommand(QueueConstants.SmsService)).Returns(jsonString);
            _eventBus.Setup(e => e.PublishEvent(It.IsAny<IEvent>())).Throws(new Exception());
            var processClientMessage = new ProcessClientMessageTask(_eventBus.Object, _sendSMSHttpClient.Object, _messageRepo.Object, _logger.Object);

            // Act
            await processClientMessage.Run();

            // Assert
            _messageRepo.Verify(repo => repo.AddMessage(It.IsAny<Message>()), Times.Once);
            _messageRepo.Verify(repo => repo.UpdateMessage(It.IsAny<Message>()), Times.Exactly(2));
            _sendSMSHttpClient.Verify(httpClient => httpClient.PostToExternalApi<IMessage>(It.IsAny<Message>()), Times.Once);
            _eventBus.Verify(bus => bus.PublishEvent(It.IsAny<SmsSentEvent>()), Times.Once);
            _logger.Verify(logger => logger.Error(It.IsAny<string>()), Times.Once);
            _logger.Verify(logger => logger.Info(It.IsAny<string>()), Times.Once);
        }
    }
}