using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.Mail
{
    /// <summary>
    /// 邮件发送人信息
    /// </summary>
    public class SenderAccount
    {
        /// <summary>
        /// 发送人邮箱
        /// </summary>
        public string senderAddress { get; set; }
        /// <summary>
        /// 发送人名字
        /// </summary>
        public string senderName { get; set; }
        /// <summary>
        /// SMTP服务器
        /// </summary>
        public string smtpHost { get; set; }
        /// <summary>
        /// SMTP端口
        /// </summary>
        public int smtpPort { get; set; }
        /// <summary>
        /// 邮箱账号名
        /// </summary>
        public string mailUsername { get; set; }
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string mailPassword { get; set; }
    }
}
