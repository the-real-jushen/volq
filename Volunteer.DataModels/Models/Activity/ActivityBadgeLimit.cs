using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class ActivityBadgeLimit
    {
        /// <summary>
        /// string为badge name
        /// 每个小list中的badge必须获得至少一个
        /// 大list中的每个小list互为“且”的关系
        /// 如{{badge1,badge2},{badge3,badge4}}，则（badge1和badge2必须满足至少一个）且（badge3和badge4必须满足至少一个）
        /// </summary>
        public List<List<string>> MustGranted { get; set; }

        /// <summary>
        /// string为badge name
        /// 获得以下badge则不能参与该活动
        /// </summary>
        public List<string> CantGranted { get; set; }

        public ActivityBadgeLimit()
        {
            MustGranted = new List<List<string>>();
            CantGranted = new List<string>();
        }
    }
}
