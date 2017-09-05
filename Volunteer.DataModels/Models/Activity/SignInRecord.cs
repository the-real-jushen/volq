using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class SignInRecord
    {
        public bool IsSignedIn { get; set; }
        public SignInRecord()
        {
            IsSignedIn = true;
        }
    }
}
