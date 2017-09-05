using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using Jtext103.Volunteer.Service;


namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\mobileapp\[actionname]
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MobileAppController : ApiControllerBase
    {
        /// <summary>
        /// 找到最新版的安卓客户端地址及更新内容
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        [ActionName("Androidapp")]
        [HttpGet]
        public HttpResponseMessage GetAndroidapp(int version)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/mobileapp/androidapp?version=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            string androidFolderPath = HttpRuntime.AppDomainAppPath + "Static\\Mobile\\Android";
            DirectoryInfo dir = new DirectoryInfo(androidFolderPath);
            int latestVersion = 1;
            List<int> versions = new List<int>();
            //android app各个版本的文件夹
            foreach (DirectoryInfo folder in dir.GetDirectories())
            {
                try
                {
                    versions.Add(Convert.ToInt32(folder.Name));
                }
                catch
                {
                    continue;
                }
            }
            //找到最新版本号
            foreach (int v in versions)
            {
                if (latestVersion < v)
                    latestVersion = v;
            }
            DownloadAndroidappModel result = new DownloadAndroidappModel();
            if (latestVersion > version)
            {
                //不是最新版本
                result.isLatest = false;
                result.changelog = File.ReadAllText(androidFolderPath + "\\Latest\\changelog.txt");
                result.downloadAppLink = "/Static/Mobile/Android/Latest/app-release.apk";
            }
            else
            {
                //已经是最新版本
                result.isLatest = true;
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
    }
    public class DownloadAndroidappModel
    {
        public bool isLatest { get; set; }
        public string changelog { get; set; }
        public string downloadAppLink { get; set; }        
    }
}
