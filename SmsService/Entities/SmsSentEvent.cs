using SharedResource.Events;

namespace SmsService.Entities
{
    public class SmsSentEvent : IEvent
    {
        public string EventType { get; set; }
    }
}
