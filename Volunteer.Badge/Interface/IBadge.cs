using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.VolunteerEvent;

namespace Jtext103.Volunteer.Badge.Interface
{
    public interface IBadge
    {
        List<BadgeDescription> GetBadgeDescription();
    }
}
