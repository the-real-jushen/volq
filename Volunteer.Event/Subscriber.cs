using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.VolunteerEvent
{
    public class Subscriber
    {
        public Subscriber(string name, List<string> triggerEventTypes, List<string> handlerNames, object triggerSender)
        {
            Id = System.Guid.NewGuid();
            Name = name;
            TriggerEventTypes = triggerEventTypes;
            HandlerNames = handlerNames;
            TriggerSender = triggerSender;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> TriggerEventTypes { get; set; }
        public object TriggerSender { get; set; }
        public List<string> HandlerNames { get; set; }
    }
}
