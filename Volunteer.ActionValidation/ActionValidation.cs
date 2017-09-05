using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.ActionValidation
{
    /// <summary>
    /// 用来验证某个动作是否合法
    /// </summary>
    public class ActionValidationModel
    {
        public ActionValidationModel(string action, object target, DateTime expireTime)
        {
            Id = System.Guid.NewGuid();
            Action = action;
            Target = target;
            ExpireTime = expireTime;
        }
        //主键，验证标志
        public Guid Id { get; set; }
        //执行的动作
        public string Action { get; set; }
        //目标（或者是内容）
        public object Target { get; set; }
        //过期时间
        public DateTime ExpireTime { get; set; }
    }
}
