using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebApiContrib.Caching;
using WebApiContrib.MessageHandlers;

namespace Jtext103.Volunteer.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 跨域访问
            config.EnableCors();
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: null,
                constraints: new { id = @"^\d+$" }
            );
            config.Routes.MapHttpRoute(
                name: "ControllerAndAction",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = "all" }
            );

            //限制web api访问次数
            config.MessageHandlers.Add(new ThrottlingHandler(
                 new InMemoryThrottleStore(),
                 id =>
                 {
                     if (id == "115.156.252.3")
                     {
                         return 50000;
                     }
                     return 100;
                 },
                 new TimeSpan(0, 2, 0)
                ));
        }
    }
}