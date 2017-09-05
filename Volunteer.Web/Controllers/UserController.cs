using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.VolunteerMessage;
using Jtext103.MongoDBProvider;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Drawing;
using System.Web;
using System.Web.Http.Cors;
using Jtext103.Volunteer.ActionValidation;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.Volunteer.Friend;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\user\[actionname]
    /// 对user的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public partial class UserController : ApiControllerBase
    {
        public UserController()
            : base()
        {

        }

        /// <summary>
        /// 获取数据库中所有的User
        /// </summary>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return GetAll("Name", true, 0, 0);
        }

        /// <summary>
        /// 获取数据库中所有的User，将结果分页排序
        /// </summary>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            List<object> Curs = new List<object>();
            foreach (var o in myService.FindAllUsers(sortByKey, isAscending, pageIndex, pageSize))
            {
                var Cur = new
                {
                    name = o.Name,
                    email = o.Email
                };
                Curs.Add(Cur);
            }
            jsonSerializer.Serialize(tw, Curs, typeof(List<User>));
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 通过email关键字获取某个User
        /// </summary>
        /// <param name="email">注册用户的邮箱</param>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetByEmail(string email)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user?email=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = myService.FindUser(email);
            if (user == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("User不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var result = new
            {
                id = user.Id,
                name = user.Name,
                avatar = user.UserProfiles.AllUserProfile.FirstOrDefault().Avatar,
                description = user.UserProfiles.AllUserProfile.FirstOrDefault().Description
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 通过ID获取某个User
        /// </summary>
        /// <param name="id">注册用户的邮箱</param>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetById(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid userId = new Guid(id);
            User user = myService.FindUser(userId);
            if (user == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("User不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, user, User.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取某用户的Information
        /// </summary>
        /// <returns></returns>
        [ActionName("Information")]
        [HttpGet]
        public HttpResponseMessage GetInformation(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/information?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = myService.FindUser(new Guid(id));
            object result;
            if (user.UserRole.FirstOrDefault() == Role.Volunteer)
            {
                var source = new
                {
                    name = user.Name,
                    avatar = user.UserProfiles[user.Name + user.UserRole.FirstOrDefault() + "Profile"].Avatar.AvatarPath,
                    role = user.UserRole.FirstOrDefault(),
                    sex = user.Sex,
                    email = user.Email,
                    IsEmailVerified = user.IsEmailVerified,
                    phoneNumber = user.PhoneNumber,
                    IsPhoneNumberVerified = user.IsPhoneNumberVerified,
                    description = user.UserProfiles.AllUserProfile.FirstOrDefault().Description,
                    affiliation = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).Affiliation,
                    location = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).Location,
                    coordinate = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).Coordinate
                };
                result = source;
            }
            else
            {
                var source = new
                {
                    name = user.Name,
                    avatar = user.UserProfiles[user.Name + user.UserRole.FirstOrDefault() + "Profile"].Avatar.AvatarPath,
                    role = user.UserRole.FirstOrDefault(),
                    sex = user.Sex,
                    email = user.Email,
                    IsEmailVerified = user.IsEmailVerified,
                    phoneNumber = user.PhoneNumber,
                    IsPhoneNumberVerified = user.IsPhoneNumberVerified,
                    description = user.UserProfiles.AllUserProfile.FirstOrDefault().Description,
                };
                result = source;
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 修改用户邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Email")]
        [HttpPut]
        public HttpResponseMessage EditEmail([FromBody]ReviseEmailModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/user/email") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = ValidationService.FindUserWithToken(GetToken());
            if (user.Email != model.email.ToLowerInvariant().Trim())
            {
                //修改的邮箱必须未被注册
                if ((myService.FindUser(model.email) == null))
                {
                    user.Email = model.email.ToLowerInvariant().Trim();
                    user.IsEmailVerified = false;
                    user.Save();
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("邮箱已被注册", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                }
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("修改后邮箱与原邮箱相同", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
        }

        /// <summary>
        /// 修改用户手机号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("PhoneNumber")]
        [HttpPut]
        public HttpResponseMessage EditPhoneNumber([FromBody]RevisePhoneNumber model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/user/phonenumber") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = ValidationService.FindUserWithToken(GetToken());
            if (user.PhoneNumber != model.phoneNumber.Trim())
            {
                user.PhoneNumber = model.phoneNumber.Trim();
                user.IsPhoneNumberVerified = false;
                user.Save();
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("修改后手机与原手机相同", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
        }
        /// <summary>
        /// 修改用户描述（个性签名）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Description")]
        [HttpPut]
        public HttpResponseMessage EditDescription([FromBody]ReviseDescriptionModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/user/description") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = ValidationService.FindUserWithToken(GetToken());
            user.UserProfiles.AllUserProfile.FirstOrDefault().Description = model.description;
            user.Save();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        ///// <summary>
        ///// 上传图像
        ///// </summary>
        ///// <returns></returns>
        //[ActionName("UploadImage")]
        //[HttpPost]
        //public HttpResponseMessage UploadImage()
        //{
        //    if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/uploadimage") == false)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    User CurrentUser = ValidationService.FindUserWithToken(GetToken());
        //    string imageName = Guid.NewGuid().ToString();//生成图像名称
        //    var httpRequest = HttpContext.Current.Request;
        //    if (httpRequest.Files.Count == 1)
        //    {
        //        var postedFile = httpRequest.Files.Get(0);
        //        string path = "/Static/Images/Avatar/" + imageName + postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("."));//相对路径+图像名称+图像格式
        //        string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径
        //        postedFile.SaveAs(filePath);

        //        string role = Enum.GetName(typeof(Role), CurrentUser.UserRole.FirstOrDefault());//获取角色名称
        //        string oldAvatarPath = HttpContext.Current.Server.MapPath("~" + CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar.AvatarPath);//根据相应Profile获得原图像相对路径
        //        if (File.Exists(oldAvatarPath) && !oldAvatarPath.Contains("default.jpg")) //如果原图像存在且不为系统默认头像，则删除
        //        {
        //            File.Delete(oldAvatarPath);
        //        }
        //        CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar.AvatarPath = path;//将新图像相对路径保存到当前用户的相应Profile中
        //        CurrentUser.Save();
        //        //生成ChangeAvatarEvent
        //        EventService.Publish("ChangeAvatarEvent", null, CurrentUser.Id);
        //        return new HttpResponseMessage(HttpStatusCode.Created);
        //    }
        //    else
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        //    }
        //}

        /// <summary>
        /// 返回图像路径
        /// </summary>
        /// <returns></returns>
        [ActionName("ImagePath")]
        [HttpGet]
        public HttpResponseMessage ImagePath()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/imagepath") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            string role = Enum.GetName(typeof(Role), CurrentUser.UserRole.FirstOrDefault());
            string imagePath = CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar.AvatarPath;
            jsonSerializer.Serialize(tw, CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar, typeof(UserAvatar));
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 手动发送验证邮件
        /// </summary>
        /// <returns></returns>
        [ActionName("SendEmailToVerify")]
        [HttpPost]
        public HttpResponseMessage SendEmailToVerify()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/sendemailtoverify") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            //生成actionValidation并发验证邮件
            var sendEmail = new SendEmail(this, currentUser, new TimeSpan(24, 0, 0), "VerifyEmail", "VerifyEmailMail.xml");
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 发送验证短信验证手机
        /// </summary>
        /// <returns></returns>
        [ActionName("SendSMSToVerify")]
        [HttpPost]
        public HttpResponseMessage SendSMSToVerify()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/sendsmstoverify") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            //生成6位数字验证码
            ValidateCode validateCode = new ValidateCode();
            string randcode = validateCode.CreateValidateCode(6);
            //发送验证短信
            try
            {
                Guid actionValidationId = SendShortMessageToVerifyPhoneNumber(currentUser, new TimeSpan(0, 10, 0), randcode, "VerifyPhoneNumber");
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, actionValidationId, actionValidationId.GetType());
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                string result = e.Message;
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, result, result.GetType());
                string jsonString = tw.ToString();
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="id">actionValidation的id</param>
        /// <returns></returns>
        [ActionName("VerifyEmail")]
        [HttpPost]
        public HttpResponseMessage VerifyEmail([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/verifyemail") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //验证actionValidation(是否过期、action是否对应)
            if (!myService.ActionValidationService.Validate(model.id, "VerifyEmail"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidationModel = myService.ActionValidationService.FindOneById(model.id);
            //从actionValidation中找到验证邮箱的userId
            Guid userId = (Guid)actionValidationModel.Target;
            User user = myService.FindUser(userId);
            //成功验证邮箱
            user.VerifyEmail();
            //将该actionValidation删除
            myService.ActionValidationService.Delete(model.id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 验证手机
        /// </summary>
        /// <param name="model">actionValidation的id，用户输入的验证码</param>
        /// <returns></returns>
        [ActionName("VerifyPhoneNumber")]
        [HttpPost]
        public HttpResponseMessage VerifyPhoneNumber([FromBody]VerifyPhoneNumberModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/verifyphonenumber") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //验证actionValidation(是否过期、action是否对应)
            if (!myService.ActionValidationService.Validate(model.actionValidationId, "VerifyPhoneNumber"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidationModel = myService.ActionValidationService.FindOneById(model.actionValidationId);
            //从actionValidation中找到验证手机的userId和短信验证码
            string[] userIdAndrandcode = ((string)actionValidationModel.Target).Split(',');
            string userid = userIdAndrandcode[0];
            string randcode = userIdAndrandcode[1];

            User currentUser = ValidationService.FindUserWithToken(GetToken());
            //当前登录用户和actionValidation中的userId不符合
            if (currentUser.Id.ToString() != userid)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            //验证码输入有误
            if (model.typingRandcode != randcode)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            //成功验证手机
            currentUser.VerifyPhoneNumber();
            //将该actionValidation删除
            myService.ActionValidationService.Delete(model.actionValidationId);
            return new HttpResponseMessage(HttpStatusCode.OK);

        }

        #region register,login,logout,validate
        /// <summary>
        /// 获取图像验证码
        /// </summary>
        /// <returns></returns>
        [ActionName("ValidateImage")]
        [HttpGet]
        public HttpResponseMessage GetValidateImage()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/validateimage") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            ValidateCode vCode = new ValidateCode();
            string code = vCode.CreateValidateCode(4);//生成验证码
            DateTime expireTime = DateTime.Now + new TimeSpan(0, 10, 0);//到期时间为当前时间之后10分钟
            ActionValidationModel actionValidate = myService.ActionValidationService.GenerateActionValidate("ValidateImage", code, expireTime);
            string base64 = vCode.CreateValidateGraphic(code);//生成验证码图片
            var result = new
            {
                id = actionValidate.Id,
                image = base64
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 完成用户注册，生成token
        /// 当用户为被邀请注册时,在用户的ExtraInformation中加入邀请人的id
        /// </summary>
        /// <param name="model">从HttpBody中获取的邮箱、用户名、密码、角色</param>
        /// <returns>注册成功返回202，失败返回401</returns>
        [ActionName("Register")]
        [HttpPost]
        public HttpResponseMessage Register([FromBody]RegisterModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/register") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //验证actionValidation(是否过期、action是否对应)，图片验证码
            if (!myService.ActionValidationService.Validate(model.id, "ValidateImage"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidationModel = myService.ActionValidationService.FindOneById(model.id);
            //验证码是否输入正确
            if (actionValidationModel.Target.ToString() != model.validateCode)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("验证码错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User u = myService.FindUser(model.email.ToLowerInvariant());
            if (u != null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("邮箱已被注册", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User newUser = new User()
            {
                Password = myService.MD5Encrypt(model.password),
                Name = model.name,
                Email = model.email.ToLowerInvariant().Trim(),
                PhoneNumber = model.phoneNumber
            };
            string profileName;
            //没有OrganizationRegister的actionValidation则只能注册为volunteer或organizer
            if (model.organizationRegisterActionValidationId == null)
            {
                switch (model.role)
                {
                    case "a":
                        newUser.UserRole.Add(Role.Volunteer);
                        profileName = newUser.Name + "VolunteerProfile";
                        newUser.AddProfile(new VolunteerProfile(profileName));
                        break;
                    case "b":
                        newUser.UserRole.Add(Role.Organizer);
                        profileName = newUser.Name + "OrganizerProfile";
                        newUser.AddProfile(new OrganizerProfile(profileName));
                        break;
                    case "c":
                        return new HttpResponseMessage(HttpStatusCode.Forbidden);
                    //newUser.UserRole.Add(Role.Organization);
                    //profileName = newUser.Name + "OrganizationProfile";
                    //newUser.AddProfile(new OrganizationProfile(profileName, 500));
                    default:
                        return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
            }
            else
            {
                //验证OrganizationRegister的actionValidation是否失效
                if (!myService.ActionValidationService.Validate(model.organizationRegisterActionValidationId, "OrganizationRegister"))
                {
                    return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
                switch (model.role)
                {
                    case "c":
                        newUser.UserRole.Add(Role.Organization);
                        profileName = newUser.Name + "OrganizationProfile";
                        newUser.AddProfile(new OrganizationProfile(profileName, 500));
                        break;
                    default:
                        return new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
                //删除该actionValidation
                myService.ActionValidationService.Delete(model.organizationRegisterActionValidationId);
            }
            switch (model.sex)
            {
                case "w":
                    newUser.Sex = Sex.Female;
                    break;
                case "m":
                    newUser.Sex = Sex.Male;
                    break;
                default:
                    newUser.Sex = Sex.Other;
                    break;
            }
            newUser.UserProfiles[profileName].Avatar.AvatarPath = "/Static/Images/Avatar/default.jpg";
            try
            {
                //当有推荐人时，在用户的ExtraInformation中加入邀请人的id
                Guid referralUserId = new Guid(model.referralUserId);
                newUser.AddExtraInformation("invited-inviteVolunteerId", referralUserId);
            }
            catch
            {
                //没有推荐人
            }
            newUser.Save();
            //产生UserRegisterEvent事件
            EventService.Publish("UserRegisterEvent", null, newUser.Id);
            //生成actionValidation并发验证邮件
            var sendEmail = new SendEmail(this, newUser, new TimeSpan(24, 0, 0), "VerifyEmail", "VerifyEmailMail.xml");
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 根据email和password完成登陆，生成token
        /// </summary>
        /// <param name="model">从HttpBody中获取的email和password</param>
        /// <returns>登陆成功返回"OK"状态、token、role、name，失败返回“ERROR”</returns>
        [ActionName("Login")]
        [HttpPost]
        public HttpResponseMessage Login([FromBody]LoginModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/login") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            string json;
            if (myService.CheckUserPassword(model.email, model.password))
            {
                User CurrentUser = myService.FindUser(model.email, model.password);
                //账号被锁定
                if (CurrentUser.IsLockedOut == true)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("账号已被锁定，请联系管理员解决！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                }
                if (IfAppLogIn() == true)
                {
                    //产生AppLoginEvent事件
                    EventService.Publish("AppLoginEvent", null, CurrentUser.Id);
                }

                TokenModel token = tokenService.RefreshTokenWhenLogIn(CurrentUser.Id, DateTime.Now.AddDays(7));

                var Cur = new
                {
                    status = "OK",
                    token = token.Id,
                    userId = CurrentUser.Id,
                    role = CurrentUser.UserRole.FirstOrDefault().ToString(),
                    name = CurrentUser.Name
                };
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, Cur, Cur.GetType());
                json = tw.ToString();
            }
            else
            {
                var Cur = new
                {
                    status = "ERROR",
                    message = "邮箱或密码错误！"
                };
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, Cur, Cur.GetType());
                json = tw.ToString();
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 完成用户登出，删除token
        /// </summary>
        /// <returns></returns>
        [ActionName("Logout")]
        [HttpPost]
        public HttpResponseMessage Logout()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/logout") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid tokenId = GetToken();
            User CurrentUser = ValidationService.FindUserWithToken(tokenId);
            //tokenService.DeleteToken(CurrentUser.Id);
            tokenService.DeleteToken(tokenId);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 判断email是否已被使用
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [ActionName("CheckEmail")]
        [HttpGet]
        public HttpResponseMessage CheckEmail(string email)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/checkemail?email=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent((myService.FindUser(email) != null).ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 对当前登录用户的token、role进行合法性验证
        /// </summary>
        /// <param name="model">从HttpBody中获取的</param>
        /// <returns>合法返回200，非法返回401、400</returns>
        [ActionName("Validate")]
        [HttpGet]
        public HttpResponseMessage ValidateUser()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/validate") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            foreach (Role role in ValidationService.FindUserWithToken(GetToken()).UserRole)
            {
                if (String.Compare(role.ToString(), GetRole(), true) == 0)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("用户不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }
        #endregion

        #region password
        /// <summary>
        /// 当前用户修改自身密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("RevisePassword")]
        public HttpResponseMessage RevisePassword([FromBody]RevisePasswordModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/user/revisepassword") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            if (model == null || model.NewPassword == null || model.OldPassword == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            if (CurrentUser.Password == myService.MD5Encrypt(model.OldPassword))
            {
                CurrentUser.Password = myService.MD5Encrypt(model.NewPassword);
                myService.SaveOne(CurrentUser);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("原密码错误！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 当前用户请求重置自身密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RequestPasswordReset")]
        public HttpResponseMessage RequestPasswordReset([FromBody]RequestPasswordResetModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/requestpasswordreset") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (model == null || model.email.Equals("") || model.validateCode.Equals("") || model.id.Equals(""))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            //验证actionValidation(是否过期、action是否对应)，图片验证码
            if (!myService.ActionValidationService.Validate(model.id, "ValidateImage"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidationModel = (ActionValidationModel)myService.ActionValidationService.FindOneById(model.id);
            //验证码是否输入正确
            if (actionValidationModel.Target.ToString() != model.validateCode)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("验证码错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User user = myService.FindUser(model.email);
            if (user != null)
            {
                //生成actionValidation并发验证邮件
                var sendEmail = new SendEmail(this, user, new TimeSpan(24, 0, 0), "VerifyEmail-ResetPassword", "ResetPasswordMail.xml");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("该邮箱未注册！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 用户邮件地址确认重置自身密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("ResetPassword")]
        public HttpResponseMessage ResetPassword([FromBody]ResetPasswordModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/user/resetpassword") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (model == null || model.id.Equals("") || model.NewPassword.Equals(""))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            //验证actionValidation(是否过期、action是否对应)
            if (!myService.ActionValidationService.Validate(model.id, "VerifyEmail-ResetPassword"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidationModel = myService.ActionValidationService.FindOneById(model.id);
            User user = (User)myService.FindOneById((Guid)actionValidationModel.Target);
            user.Password = myService.MD5Encrypt(model.NewPassword);
            myService.SaveOne(user);
            //删除该actionValidation
            myService.ActionValidationService.Delete(model.id);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }
        #endregion

        #region message
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="sendMessageModel"></param>
        /// <returns></returns>
        [ActionName("SendMessage")]
        [HttpPost]
        public HttpResponseMessage SendMessage([FromBody]SendMessageModel sendMessageModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/sendmessage") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            myService.MessageService.SendMessage(currentUser.Id.ToString(), new Guid(sendMessageModel.receiverId), sendMessageModel.title, sendMessageModel.text, null, null, false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 将一条信息标记为已读
        /// </summary>
        /// <param name="messageIdModel"></param>
        /// <returns></returns>
        [ActionName("ReadMessage")]
        [HttpPost]
        public HttpResponseMessage ReadMessage([FromBody]MessageIdModel messageIdModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/readmessage") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            myService.MessageService.ReadMessage(new Guid(messageIdModel.messageId), currentUser.Id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 将一条信息标记为已删除
        /// </summary>
        /// <param name="messageIdModel"></param>
        /// <returns></returns>
        [ActionName("DeleteMessage")]
        [HttpDelete]
        public HttpResponseMessage DeleteMessage(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "delete:/api/user/deletemessage?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            myService.MessageService.DeleteMessage(new Guid(id));
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 找到所有我的信息
        /// </summary>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("MyMessages")]
        [HttpGet]
        public HttpResponseMessage GetMyMessages(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/mymessages?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            IEnumerable<Message> result = myService.MessageService.FindMyMessages(CurrentUser.Id, sortByKey, isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 找到所有我已读或未读的信息
        /// </summary>
        /// <param name="hasRead"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("MyMessages")]
        [HttpGet]
        public HttpResponseMessage GetMyMessages(bool hasRead, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/mymessages?hasread=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            IEnumerable<Message> result = myService.MessageService.FindMyReadOrNotReadMessage(CurrentUser.Id, null, hasRead, sortByKey, isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        #endregion

        #region feed
        /// <summary>
        /// 将一条feed标记为已读
        /// </summary>
        /// <param name="feedIdModel"></param>
        /// <returns></returns>
        [ActionName("ReadFeed")]
        [HttpPost]
        public HttpResponseMessage ReadFeed([FromBody]FeedIdModel feedIdModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/user/readfeed") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            myService.FeedService.ReadMessage(new Guid(feedIdModel.feedId), currentUser.Id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 获得指定用户id的feed
        /// </summary>
        /// <returns></returns>
        [ActionName("MyFeeds")]
        [HttpGet]
        public HttpResponseMessage GetMyFeeds(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/user/myfeeds?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid userId = new Guid(id);
            User user = myService.FindUser(userId);
            //如果当前用户和user都是volunteer，必须是自己或者好友才能调用该web api看到活动
            if (user.UserRole.Contains(Role.Volunteer) && currentUser.UserRole.Contains(Role.Volunteer))
            {
                if (currentUser.Id != userId)
                {
                    if (FriendService.CheckIfWeAreFriends(currentUser.Id, userId) == false)
                    {
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                    }
                }
            }
            IEnumerable<Message> messages;
            //当前用户看自己的feed
            if (currentUser.Id == userId)
            {
                messages = myService.FeedService.FindMyReadOrNotReadMessage(userId, null, false, sortByKey, isAscending, pageIndex, pageSize);
            }
            //当前用户看好友的feed
            //得到的是以好友为messageFrom的当前用户的feed，无论是否已读
            else
            {
                messages = myService.FeedService.FindMyReadOrNotReadMessage(currentUser.Id, userId.ToString(), false, sortByKey, isAscending, pageIndex, pageSize);
                messages.Union(myService.FeedService.FindMyReadOrNotReadMessage(currentUser.Id, userId.ToString(), true, sortByKey, isAscending, pageIndex, pageSize));
            }
            List<FeedModel> result = new List<FeedModel>();
            foreach (var message in messages)
            {
                string fromAvatar, fromName, fromLink;
                switch (message.MessageFrom)
                {
                    case "System":
                        fromAvatar = "/Static/Images/Avatar/server.png";
                        fromName = "System";
                        fromLink = "javascript:void(0);";
                        break;
                    case "Admin":
                        fromAvatar = "/Static/Images/Avatar/administrator.png";
                        fromName = "Admin";
                        fromLink = "javascript:void(0);";
                        break;
                    default:
                        User messageFromUser = (User)myService.FindOneById(new Guid(message.MessageFrom));
                        string role = Enum.GetName(typeof(Role), messageFromUser.UserRole.FirstOrDefault());
                        fromAvatar = messageFromUser.UserProfiles[messageFromUser.Name + role + "Profile"].Avatar.AvatarPath;
                        fromName = messageFromUser.Name;
                        fromLink = "visitor.html?id=" + message.MessageFrom;
                        break;
                }
                //是否在新窗口打开
                string linkTarget;
                if (message.NewBlank == true)
                {
                    linkTarget = "_blank";
                }
                else
                {
                    linkTarget = "_self";
                }
                FeedModel feed = new FeedModel
                {
                    Id = message.Id,
                    Title = message.Title,
                    Text = message.Text,
                    Pictures = message.Pictures,
                    DestinationLink = message.DestinationLink,
                    LinkTarget = linkTarget,
                    Time = message.Time,
                    MessageFrom = message.MessageFrom,
                    FromName = fromName,
                    FromAvatar = fromAvatar,
                    FromLink = fromLink
                };
                result.Add(feed);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        #endregion feed
    }

    public class IdModel
    {
        public string id { get; set; }
    }

    public class RegisterModel
    {
        public string id { get; set; }
        public string validateCode { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string sex { get; set; }
        public string phoneNumber { get; set; }
        public string referralUserId { get; set; }
        public string organizationRegisterActionValidationId { get; set; }
    }

    public class LoginModel
    {
        public string email { get; set; }
        public string password { get; set; }
    }

    public class RoleModel
    {
        public string role { get; set; }
    }
    public class SendMessageModel
    {
        public string receiverId { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }
    public class MessageIdModel
    {
        public string messageId { get; set; }
    }
    public class FeedIdModel
    {
        public string feedId { get; set; }
    }
    public class FeedModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> Pictures { get; set; }
        public string DestinationLink { get; set; }
        public string LinkTarget { get; set; }
        public DateTime Time { get; set; }
        public string MessageFrom { get; set; }
        public string FromName { get; set; }
        public string FromAvatar { get; set; }
        public string FromLink { get; set; }
        public string FromTarget { get; set; }
    }
    public class RevisePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class RequestPasswordResetModel
    {
        public string id { get; set; }
        public string validateCode { get; set; }
        public string email { get; set; }
    }
    public class ResetPasswordModel
    {
        public string id { get; set; }
        public string NewPassword { get; set; }
    }
    public class ReviseDescriptionModel
    {
        public string description { get; set; }
    }
    public class ReviseEmailModel
    {
        public string email { get; set; }
    }
    public class RevisePhoneNumber
    {
        public string phoneNumber { get; set; }
    }
    public class VerifyPhoneNumberModel
    {
        /// <summary>
        /// actionValidation的id
        /// </summary>
        public string actionValidationId { get; set; }
        /// <summary>
        /// 用户输入的验证码
        /// </summary>
        public string typingRandcode { get; set; }
    }
}