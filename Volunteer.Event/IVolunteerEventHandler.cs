using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer;

namespace Jtext103.Volunteer.VolunteerEvent
{
    public interface IVolunteerEventHandler
    {
        string Name { get; }
        //handle the event
        void HandleEvent(Event oneEvent);
        List<Subscriber> GetHandlerSubscribers();
    }
}
