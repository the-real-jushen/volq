using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Jtext103.StringConfig;

namespace Jtext103.Volunteer.Mail
{
    public class MailService
    {
        public static MailService Instance;
        private SenderAccount senderAccount { get; set; }

        /// <summary>
        /// MailService构造函数
        /// </summary>
        /// <param name="xmlFileName">xml配置文件名</param>
        public MailService(string xmlFileName)
        {
            senderAccount = new SenderAccount
            {
                senderAddress = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "senderAddress"),
                senderName = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "senderName"),
                smtpHost = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "smtpHost"),
                smtpPort = Convert.ToInt32(Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "smtpPort")),
                mailUsername = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "mailUsername"),
                mailPassword = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "mailPassword"),
            };
            Instance = this;
        }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="receivers">接收者信息（可以有多个接收者）</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        public bool SendMail(IEnumerable<string> receivers, string subject, string body, IEnumerable<string> attachments)
        {
            MailAddress from = new MailAddress(senderAccount.senderAddress, senderAccount.senderName);
            if (receivers.Any())
            {
                foreach (string receiver in receivers)
                {
                    //receiver的格式为 aaa<bbb@ccc.ddd>，尖括号外（aaa）为接收者名字，尖括号内（bbb@ccc.ddd）为邮件地址
                    //如果没有接收者名字，则receiver格式为bbb@ccc.ddd，可以认为bbb为接收者名字
                    string receiverAddress;
                    string receiverName;
                    if(receiver.Contains('<'))
                    {
                        string[] receiverNameAndAddress = receiver.Split('<');
                        receiverName = receiverNameAndAddress[0];
                        receiverAddress = receiverNameAndAddress[1].TrimEnd('>');
                    }
                    else
                    {
                        string[] receiverNameAndAddress = receiver.Split('@');
                        receiverName = receiverNameAndAddress[0];
                        receiverAddress = receiver;
                    }
                    MailAddress to = new MailAddress(receiverAddress, receiverName);

                    MailMessage mail = new MailMessage(from, to);
                    if (attachments.Any())
                    {
                        foreach (string attachment in attachments)
                        {
                            mail.Attachments.Add(new Attachment(attachment));
                        }
                    }
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                    mail.Priority = MailPriority.Normal;
                    //发送邮件
                    SmtpClient client = new SmtpClient();
                    client.Host = senderAccount.smtpHost;
                    client.Port = senderAccount.smtpPort;
                    client.Credentials = new NetworkCredential(senderAccount.mailUsername, senderAccount.mailPassword);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    try
                    {
                        client.Send(mail);
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        //释放资源
                        mail.Dispose();
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public bool SendMail(IEnumerable<string> receivers, string subject, string body)
        {
            List<string> attachments = new List<string>();
            return SendMail(receivers, subject, body, attachments);
        }
        public bool SendMail(string receiver, string subject, string body, IEnumerable<string> attachments)
        {
            List<string> receivers = new List<string>();
            receivers.Add(receiver);
            return SendMail(receivers, subject, body, attachments);
        }
        public bool SendMail(string receiver, string subject, string body)
        {
            List<string> receivers = new List<string>();
            receivers.Add(receiver);
            List<string> attachments = new List<string>();
            return SendMail(receivers, subject, body, attachments);
        }
    }
}
