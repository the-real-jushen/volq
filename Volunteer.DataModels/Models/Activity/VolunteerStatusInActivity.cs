using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    //volunteer参加某一活动时的状态
    public enum VolunteerStatusInActivity
    {
        /// <summary>
        /// 已经signin, 但还未checkin
        /// </summary>
        signedIn,

        /// <summary>
        /// signin后，未checkin，手动退出
        /// </summary>
        unsignedIn,

        /// <summary>
        /// 已经checkin，但还未checkout（活动进行中）
        /// </summary>
        checkedIn,

        /// <summary>
        /// 只signin，活动结束时还未checkin（即未参加活动）
        /// </summary>
        notParticipateIn,

        /// <summary>
        /// 中途退出（只check in后，未完成活动）
        /// </summary>
        quit,

        /// <summary>
        /// checkout并顺利完成
        /// </summary>
        complete,

        /// <summary>
        /// 错误
        /// </summary>
        error,

        /// <summary>
        /// 被管理者踢出
        /// </summary>
        kickedOut

    }
}
