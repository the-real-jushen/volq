using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class CheckOutRecord
    {
        public bool IsCheckedOut { get; set; }
        public DateTime CheckedOutTime { get; set; }
        public CheckOutStatus Status { get; set; }
        public CheckOutRecord()
        {
            IsCheckedOut = false;
        }
    }
}
