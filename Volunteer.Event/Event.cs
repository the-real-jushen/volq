using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.VolunteerEvent
{
    public class Event
    {
        public Event(string type, string value, object sender)
        {
            Id = System.Guid.NewGuid();
            Type = type;
            Value = value;
            Sender = sender;
            hasHandled = false;
        }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public object Sender { get; set; }
        public bool hasHandled { get; set; }
    }
}
