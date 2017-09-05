using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class CheckInRecord
    {
        public bool IsCheckedIn { get; set; }
        public DateTime CheckedInTime { get; set; }

        //in the future you can add something like checkin location
        //checked in by which organizer etc.
        public CheckInRecord()
        {
            IsCheckedIn = false;
        }
    }
}
