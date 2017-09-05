using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public enum CheckOutStatus
    {
        /// <summary>
        /// checkout并顺利完成
        /// </summary>
        complete,

        /// <summary>
        /// 中途退出
        /// </summary>
        quit,
    }
}
