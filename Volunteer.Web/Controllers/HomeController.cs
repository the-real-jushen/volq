using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace Jtext103.Volunteer.Web.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 若已登陆，根据角色打开相应页面
        /// 未登录：index.html
        /// </summary>
        public void Index()
        {
            HttpCookieCollection cookies = Request.Cookies;
            try {
                string role = cookies["role"].Value;
                switch (role)
                {
                    case "Volunteer":
                        Response.Redirect("~/Views/volunteer.html");
                        break;
                    case "Organizer":
                        Response.Redirect("~/Views/organizer.html");
                        break;
                    case "Organization":
                        Response.Redirect("~/Views/organization.html");
                        break;
                    default:
                        Response.Redirect("~/Views/index.html");
                        break;
                }
            }
            catch(Exception)
            {
                Response.Redirect("~/Views/signin.html");  
            }
        }
        /// <summary>
        /// Web Api说明文档
        /// </summary>
        public void List(string command)
        {
            Response.Redirect("~/WebApiList.html?command=" + command);
        }
        /// <summary>
        /// Qunit对Web Api进行测试
        /// </summary>
        public void Test()
        {
            Response.Redirect("~/Test/webapi/webapi.testsuite.html");
        }

    }
}