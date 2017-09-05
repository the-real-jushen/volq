using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public enum ActivityStatus
    {
        //草案
        Draft,
        //激活（approved）
        Active,
        //可以sign in
        SignIn,
        //报名人数达到上限
        MaxVolunteer,
        //准备就绪
        Ready,
        //可以check in
        RunningCheckIn,
        //可以sign in也可以check in
        RunningSignInAndCheckIn,
        //已结束
        Finished,
        //已取消
        Abort
    }
}
