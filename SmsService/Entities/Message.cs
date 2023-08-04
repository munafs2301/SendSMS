using SharedResource.Events;
using SmsService.Constants;

namespace SmsService.Entities
{
    public class Message : IMessage
    {
        public string PhoneNumber { get; set; }

        public string SmsText { get; set; }
        public MessageStatus Status { get; set; }
    }
}
