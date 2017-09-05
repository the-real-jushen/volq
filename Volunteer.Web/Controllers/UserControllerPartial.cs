using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Jtext103.Volunteer.ActionValidation;
using Jtext103.Volunteer.Web.Controllers;
using Jtext103.StringConfig;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.ShortMessage;
using System.Threading;

namespace Jtext103.Volunteer.Web.Controllers
{
    public partial class UserController : ApiControllerBase
    {
        /// <summary>
        /// 发送验证邮件，生成对应的actionValidation
        /// </summary>
        /// <param name="user">发送邮件目标用户</param>
        /// <param name="expireTimeSpan">链接过期时间</param>
        /// <param name="actionValidationAction">actionValidation的action</param>
        /// <param name="xmlFileName">xml配置文件名</param>
        /// <param name="replaceDic">替换模板的dictionary</param>
        internal void SendEmailToVerifyMail(User user, TimeSpan expireTimeSpan, string actionValidationAction, string xmlFileName)
        {
            //到期时间
            DateTime expireTime = DateTime.Now + expireTimeSpan;
            //生成actionValidation
            ActionValidationModel actionValidate = myService.ActionValidationService.GenerateActionValidate(actionValidationAction, user.Id, expireTime);
            //从xml中读取邮件主题
            string subject = Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "MailSubject");
            //从xml中读取模板路径
            string templatePath = HttpRuntime.AppDomainAppPath + Jtext103.StringConfig.ConfigString.GetString(xmlFileName, "MailTemplateRelativePath");
            //读取模板
            string template = File.ReadAllText(templatePath);
            //替换模板关键字
            Dictionary<string, string> replaceDic = new Dictionary<string, string>();
            replaceDic.Add("actionValidateId", actionValidate.Id.ToString());
            string mailContent = GenerateStringFromTemplate.GenerateString(template, replaceDic);
            //发送邮件
            mailService.SendMail(user.Email, subject, mailContent);
        }

        /// <summary>
        /// 发送验证短信，生成对应的actionValidation
        /// </summary>
        /// <param name="user">发送短信目标用户</param>
        /// <param name="expireTimeSpan">短信验证码过期时间(整分钟)</param>
        /// <param name="randcode">短信验证码</param>
        /// <param name="actionValidationAction">actionValidation的action</param>
        /// <returns>生成actionValidation的id</returns>
        internal Guid SendShortMessageToVerifyPhoneNumber(User user, TimeSpan expireTimeSpan, string randcode, string actionValidationAction)
        {
            //string lastsend;
            //if (user.ExtraInformation.ContainsKey("sendSMS-lastTime") == false)
            //{
            //    lastsend = "null";
            //}
            //else
            //{
            //    lastsend=((DateTime)user.ExtraInformation["sendSMS-lastTime"]).ToLocalTime().ToString();
            //}
            //System.Diagnostics.Debug.WriteLine("last Send:" + lastsend);
            //System.Diagnostics.Debug.WriteLine("Now:" + DateTime.Now);
            if (user.ExtraInformation.ContainsKey("sendSMS-lastTime") == false || DateTime.Now - ((DateTime)user.ExtraInformation["sendSMS-lastTime"]).ToLocalTime() >= new TimeSpan(0, 5, 0))
            {
                
                //到期时间
                DateTime expireTime = DateTime.Now + expireTimeSpan;
                int expireTimeTotalMinutes = Convert.ToInt32(expireTimeSpan.TotalMinutes);
                //生成actionValidation
                ActionValidationModel actionValidate = myService.ActionValidationService.GenerateActionValidate(actionValidationAction, user.Id.ToString() + "," + randcode, expireTime);
                //发送验证短信
                shortMessageService.SendShortMessage(user.PhoneNumber, randcode, expireTimeTotalMinutes);
                //修改用户extra中信息
                user.ModifyExtraInformation("sendSMS-lastTime", DateTime.Now);
                //System.Diagnostics.Debug.WriteLine("This Send:" + ((DateTime)(myService.FindUser(user.Id).ExtraInformation["sendSMS-lastTime"])).ToLocalTime());
                return actionValidate.Id;
            }
            else
            {
                throw new Exception("两次发送验证短信间隔不能小于5分钟");
            }
        }
    }
    //新建线程，发送邮件
    internal class SendEmail
    {
        private UserController userController;
        private User user;
        private TimeSpan expireTimeSpan;
        private string actionValidationAction;
        private string xmlFileName;
        public SendEmail(UserController userController, User user, TimeSpan expireTimeSpan, string actionValidationAction, string xmlFileName)
        {
            this.userController = userController;
            this.user = user;
            this.expireTimeSpan = expireTimeSpan;
            this.actionValidationAction = actionValidationAction;
            this.xmlFileName = xmlFileName;
            Thread sendEmailThread = new Thread(this.send);
            sendEmailThread.Start();
        }
        private void send()
        {
            userController.SendEmailToVerifyMail(user, expireTimeSpan, actionValidationAction, xmlFileName);
        }
    }
    //新建线程，发送验证短信
    internal class SendShortMessageToVerify
    {
        private UserController userController;
        private User user;
        private TimeSpan expireTimeSpan;
        private string randcode;
        private string actionValidationAction;
        public SendShortMessageToVerify(UserController userController, User user, TimeSpan expireTimeSpan, string randcode, string actionValidationAction)
        {
            this.userController = userController;
            this.user = user;
            this.expireTimeSpan = expireTimeSpan;
            this.randcode = randcode;
            this.actionValidationAction = actionValidationAction;
            Thread sendShortMessageThread = new Thread(this.send);
            sendShortMessageThread.Start();
        }
        private void send()
        {
            userController.SendShortMessageToVerifyPhoneNumber(user, expireTimeSpan, randcode, actionValidationAction);
        }
    }
}