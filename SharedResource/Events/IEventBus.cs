using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedResource.Events
{
    public interface IEventBus
    {
        string ConsumeCommand(string queue);
        void PublishCommand(IMessage message, string queue);
        void PublishEvent(IEvent @event);
    }
}
