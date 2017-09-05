using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Net;
using System.IO;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.ImageHandler;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\content\[actionname]
    /// 对内容的相关操作
    /// </summary>
    public class ContentController : ApiControllerBase
    {
        public ContentController()
            : base()
        {

        }

        /// <summary>
        /// 上传头像
        /// 如果头像过大，则默认裁剪为128*128
        /// </summary>
        /// <returns></returns>
        [ActionName("UploadAvatar")]
        [HttpPost]
        public HttpResponseMessage UploadAvatar()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/content/uploadavatar") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count != 1)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            string imageName = Guid.NewGuid().ToString();//生成图像名称
            var postedFile = httpRequest.Files[0];
            string path = "/Static/Images/Avatar/" + imageName + postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("."));//相对路径+图像名称+图像格式
            string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径

            Stream fileStream = postedFile.InputStream;
            HandleImageService.CutForCustom(fileStream, filePath, 128, 128, 75);//剪裁为128*128并保存图像到本地
            fileStream.Close();

            string role = Enum.GetName(typeof(Role), CurrentUser.UserRole.FirstOrDefault());//获取角色名称
            string oldAvatarPath = HttpContext.Current.Server.MapPath("~" + CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar.AvatarPath);//根据相应Profile获得原图像相对路径
            if (File.Exists(oldAvatarPath) && !oldAvatarPath.Contains("default.jpg")) //如果原图像存在且不为系统默认头像，则删除
            {
                File.Delete(oldAvatarPath);
            }
            CurrentUser.UserProfiles[CurrentUser.Name + role + "Profile"].Avatar.AvatarPath = path;//将新图像相对路径保存到当前用户的相应Profile中
            CurrentUser.Save();
            //生成ChangeAvatarEvent
            EventService.Publish("ChangeAvatarEvent", null, CurrentUser.Id);
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        /// <summary>
        /// 上传活动图片
        /// 如果图片过大，默认裁剪为960*720
        /// </summary>
        /// <returns></returns>
        [ActionName("UploadActivityImage")]
        [HttpPost]
        public HttpResponseMessage UploadActivityImage()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/content/uploadactivityimage") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count != 1)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("error", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var postedFile = httpRequest.Files[0];
            string imageName = Guid.NewGuid().ToString();//生成图像名称
            string path = "/Static/Images/Activity/" + imageName + postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("."));//相对路径+图像名称+图像格式
            string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径

            Stream fileStream = postedFile.InputStream;
            HandleImageService.CutForCustom(fileStream, filePath, 960, 720, 75);//剪裁为960*720并保存图像到本地
            fileStream.Close();

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new StringContent(path, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 用户提交意见，发送邮件到service@volq.org
        /// </summary>
        /// <returns></returns>
        [ActionName("Suggestion")]
        [HttpPost]
        public HttpResponseMessage PostSuggestion([FromBody]ContentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/content/suggestion") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            if (!mailService.SendMail("feedback@volq.org", "From:" + currentUser.Email, model.content))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
    public class ContentModel
    {
        public string content { get; set; }
    }
}