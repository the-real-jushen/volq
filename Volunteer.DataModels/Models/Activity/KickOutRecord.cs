using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class KickOutRecord
    {
        public bool IsKickedOut { get; set; }
        public DateTime KickedOutTime { get; set; }
        public KickOutRecord()
        {
            IsKickedOut = false;
        }
    }
}
