using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class VolunteerParticipateInActivityRecord
    {
        //volunteer的id
        public Guid VolunteerId { get; set; }
        //volunteer sign in 的时间
        public DateTime SignInTime { get; set; }
        //sign in状态
        public SignInRecord SignedIn { get; set; }
        //check in状态
        public CheckInRecord CheckedIn { get; set; }
        //check out状态
        public CheckOutRecord CheckedOut { get; set; }
        //kick out状态
        public KickOutRecord KickedOut { get; set; }
        //volunteer对活动进行打分，初始值为NaN，代表未打过分
        public double Rate { get; set; }
        //是否已经打过分
        public bool HasRated { get; set; }
        //volunteer参加某一活动时的状态
        public VolunteerStatusInActivity VolunteerStatus
        {
            get
            {
                if (KickedOut.IsKickedOut == true)
                {
                    return VolunteerStatusInActivity.kickedOut;
                }
                else if (SignedIn.IsSignedIn == false)
                {
                    return VolunteerStatusInActivity.unsignedIn;
                }
                else
                {
                    if (CheckedIn.IsCheckedIn == false && CheckedOut.IsCheckedOut == false)
                    {
                        return VolunteerStatusInActivity.signedIn;
                    }
                    if (CheckedIn.IsCheckedIn == false && CheckedOut.IsCheckedOut == true)
                    {
                        return VolunteerStatusInActivity.notParticipateIn;
                    }
                    if (CheckedIn.IsCheckedIn == true && CheckedOut.IsCheckedOut == false)
                    {
                        return VolunteerStatusInActivity.checkedIn;
                    }
                    if (CheckedIn.IsCheckedIn == true && CheckedOut.IsCheckedOut == true)
                    {
                        if (CheckedOut.Status == CheckOutStatus.complete)
                        {
                            return VolunteerStatusInActivity.complete;
                        }
                        if (CheckedOut.Status == CheckOutStatus.quit)
                        {
                            return VolunteerStatusInActivity.quit;
                        }
                    }
                    return VolunteerStatusInActivity.error;
                }
            }
        }

        public VolunteerParticipateInActivityRecord(Guid volunteerId, DateTime signInTime)
        {
            VolunteerId = volunteerId;
            SignInTime = signInTime;
            SignedIn = new SignInRecord();
            CheckedIn = new CheckInRecord();
            CheckedOut = new CheckOutRecord();
            KickedOut = new KickOutRecord();
            Rate = 0;
            HasRated = false;
        }
    }
}
