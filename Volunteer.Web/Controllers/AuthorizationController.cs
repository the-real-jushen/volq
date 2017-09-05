using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.Service;
using Jtext103.MongoDBProvider;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Drawing;
using System.Web;

namespace Jtext103.Volunteer.Web.Controllers
{
    public class AuthorizationController : ApiControllerBase
    {
        /// <summary>
        /// 获取所有webapi
        /// </summary>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll(string command)
        {
            string _command = "jtext" + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString();
            IEnumerable<AuthorizationModel> authorizationModels = ValidationService.FindAllAuthorizationModel();
            var result = new {
                user = new List<AuthorizationModel>(),
                volunteer = new List<AuthorizationModel>(),
                organization = new List<AuthorizationModel>(),
                organizer = new List<AuthorizationModel>(),
                activity = new List<AuthorizationModel>(),
                badge = new List<AuthorizationModel>(),
                mobileapp = new List<AuthorizationModel>(),
                content = new List<AuthorizationModel>()
            };
            if (string.Equals(command, _command, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var authorizationModel in authorizationModels)
                {
                    string kind, apiName = authorizationModel.ApiName;
                    int start = apiName.IndexOf("api/") + 4;
                    if (apiName.IndexOf("/", start) > 0)
                    {
                        int length = apiName.IndexOf("/", start) - start;
                        kind = apiName.Substring(start, length);
                    }
                    else if (apiName.IndexOf("?", start) > 0)
                    {
                        int length = apiName.IndexOf("?", start) - start;
                        kind = apiName.Substring(start, length);
                    }
                    else
                    {
                        kind = apiName.Substring(start);
                    }
                    switch (kind)
                    {
                        case "user":
                            result.user.Add(authorizationModel);
                            break;
                        case "volunteer":
                            result.volunteer.Add(authorizationModel);
                            break;
                        case "organization":
                            result.organization.Add(authorizationModel);
                            break;
                        case "organizer":
                            result.organizer.Add(authorizationModel);
                            break;
                        case "activity":
                            result.activity.Add(authorizationModel);
                            break;
                        case "badge":
                            result.badge.Add(authorizationModel);
                            break;
                        case "mobileapp":
                            result.mobileapp.Add(authorizationModel);
                            break;
                        case "content":
                            result.content.Add(authorizationModel);
                            break;
                        default:
                            break;
                    }
                }
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
    }
}