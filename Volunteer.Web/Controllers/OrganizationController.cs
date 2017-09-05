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
using System.Web.Http.Cors;
using Jtext103.Volunteer.VolunteerMessage;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\organization\[actionname]
    /// 对Organization的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OrganizationController : ApiControllerBase
    {
        public OrganizationController()
            : base()
        {

        }

        /// <summary>
        /// 获取所有的Organization
        /// </summary>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return GetAll("Name", true, 0, 0);
        }

        /// <summary>
        /// 获取所有的Organization，将结果分页排序
        /// </summary>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<object> Curs = new List<object>();
            foreach (var o in myService.FindAllOrganizations(sortByKey, isAscending, pageIndex, pageSize))
            {
                var Cur = new
                {
                    name = o.Name,
                    id = o.Id
                };
                Curs.Add(Cur);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, Curs, Curs.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取当前organization所有(organizer)成员
        /// </summary>
        /// <returns></returns>
        [ActionName("Members")]
        [HttpGet]
        public HttpResponseMessage Members()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/members") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return Members("Name", true, 0, 0);
        }

        /// <summary>
        /// 获取当前organization所有(organizer)成员，将结果分页排序
        /// </summary>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("Members")]
        [HttpGet]
        public HttpResponseMessage Members(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/members?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            List<object> Curs = new List<object>();
            foreach (var organizer in myService.FindAllOrganizerByOrganization(organization, sortByKey, isAscending, pageIndex, pageSize))
            {
                var Cur = new
                {
                    organizerName = organizer.Name,
                    organizerId = organizer.Id,
                    email = organizer.Email,
                    time = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations[organization.Id],
                    avatar = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).Avatar
                };
                Curs.Add(Cur);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, Curs, Curs.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 该organization踢出(organizer)成员，成功则分别给organization和organizer发送信息
        /// </summary>
        [ActionName("KickOut")]
        [HttpPost]
        public HttpResponseMessage KickOut([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organization/kickout") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            User organizer = (User)myService.FindOneById(new Guid(model.id));
            if (myService.OrganizationKickOrganizerOut(organizer, organization, model.comment))
            {
                //myService.MessageService.SendMessage("System", organizer.Id, "你被组织踢出", "你被组织" + organization.Name + "踢出了", null, null);
                //myService.MessageService.SendMessage("System", organization.Id, "你将某人踢出了组织", organizer.Name + "被踢出了组织", null, null);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Service错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 获取所有申请当前organization的organizer
        /// </summary>
        /// <returns></returns>
        [ActionName("AppliedOrganizers")]
        [HttpGet]
        public HttpResponseMessage AppliedOrganizers()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/appliedorganizers") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return AppliedOrganizers("Name", true, 0, 0);
        }

        /// <summary>
        /// 获取所有申请当前organization的organizer，将结果分页排序
        /// </summary>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("AppliedOrganizers")]
        [HttpGet]
        public HttpResponseMessage AppliedOrganizers(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/appliedorganizers?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            List<object> Curs = new List<object>();
            foreach (var o in myService.FindAllAppliedOrganizerByOrganization(organization, sortByKey, isAscending, pageIndex, pageSize))
            {
                var Cur = new
                {
                    name = o.Name,
                    id = o.Id
                };
                Curs.Add(Cur);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, Curs, Curs.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 当前organization同意organizer的加入申请，成功则分别给organization和organizer发送信息
        /// </summary>
        /// <param name="model">想要加入的(organizer)成员id</param>
        /// <returns></returns>
        [ActionName("AcceptToJoin")]
        [HttpPost]
        public HttpResponseMessage AcceptToJoin([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organization/accepttojoin") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            User organizer = (User)myService.FindOneById(new Guid(model.id));
            if (myService.OrganizationAcceptOrganizerApplication(organizer, organization, model.comment))
            {
                //myService.MessageService.SendMessage("System", organizer.Id, "你加入了组织", "你加入了组织" + organization.Name, null, null);
                //myService.MessageService.SendMessage("System", organization.Id, "有人加入了组织", organizer.Name + "加入了组织", null, null);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Service错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 当前organization拒绝organizer的加入申请，成功则分别给organization和organizer发送信息
        /// </summary>
        /// <param name="model">想要加入的(organizer)成员id</param>
        /// <returns></returns>
        [ActionName("RefuseToJoin")]
        [HttpPost]
        public HttpResponseMessage RefuseToJoin([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organization/refusetojoin") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            User organizer = (User)myService.FindOneById(new Guid(model.id));
            if (myService.OrganizationRefuseOrganizerApplication(organizer, organization, model.comment))
            {
                //myService.MessageService.SendMessage("System", organizer.Id, "你加入组织的申请被拒绝", "你加入组织" + organization.Name + "的申请被拒绝了", null, null);
                //myService.MessageService.SendMessage("System", organization.Id, "你拒绝某人加入组织的申请", organizer.Name + "加入组织的申请被你拒绝了", null, null);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Service错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 申请该organization的历史记录
        /// </summary>
        /// <returns></returns>
        [ActionName("AppliedHistory")]
        [HttpGet]
        public HttpResponseMessage AppliedHistory()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/appliedhistory") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return AppliedHistory("time", false, 0, 0);
        }
        //排序并分页
        [ActionName("AppliedHistory")]
        [HttpGet]
        public HttpResponseMessage AppliedHistory(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/appliedhistory?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            List<object> source = new List<object>();
            foreach (var information in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation)
            {
                User organizer = (User)myService.FindOneById(information.OrganizerId);
                if (information.hasHandled == false)
                {
                    source.Add(new AppliedToMeHistoryModel
                    { 
                        organizerId = information.OrganizerId,
                        organizerName = information.OrganizerName,
                        time = information.Time,
                        comment = information.Comment,
                        avatar = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).Avatar
                    });
                }
            }
            List<object> result = myService.SortAndPaging(source, sortByKey, isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 为organization设置每月point总数
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("SetTotalPoint")]
        [HttpPost]
        public HttpResponseMessage SetOrganizationTotalPointEachMonth([FromBody]SetPointModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organization/settotalpoint") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = myService.FindUser(new Guid(model.id));
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).TotalPointEachMonth = model.point;
            organization.Save();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 为organization设置当月point余额
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("SetRemainingPoint")]
        [HttpPost]
        public HttpResponseMessage SetOrganizationRemainingPoint([FromBody]SetPointModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organization/setremainingpoint") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = myService.FindUser(new Guid(model.id));
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum = model.point;
            organization.Save();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 获取organization的当月point余额
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("RemainingPoint")]
        [HttpGet]
        public HttpResponseMessage GetOrganizationRemainingPoint(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/remainingpoint?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = myService.FindUser(new Guid(id));
            double remainingSum = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum;
            return new HttpResponseMessage { Content = new StringContent(remainingSum.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 获得某Organization的统计数据
        /// </summary>
        /// <returns></returns>
        [ActionName("Statistics")]
        [HttpGet]
        public HttpResponseMessage GetOrganizationStatistics(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organization/statistics?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = myService.FindUser(new Guid(id));
            var result = new
            {
                TotalPointEachMonth = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).TotalPointEachMonth,
                RemainingSum = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum,
                ConsumeAllPoint = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ConsumeAllPoint,
                AllActivityCount = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllActivityCount,
                AllVolunteerCount = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllVolunteerCount,
                StatisticsPerMonth = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizationStatistics
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        public class IdModel
        {
            public string id { get; set; }
        }

        public class IdAndCommentModel
        {
            public string id { get; set; }
            public string comment { get; set; }
        }

        public class SetPointModel
        {
            public string id { get; set; }
            public double point { get; set; }
        }
        public class AppliedToMeHistoryModel
        {
            public Guid organizerId { get; set; }
            public string organizerName { get; set; }
            public DateTime time { get; set; }
            public string comment { get; set; }
            public UserAvatar avatar { get; set; }
        }
    }
}