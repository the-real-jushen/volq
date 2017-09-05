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
using Jtext103.Volunteer.ActionValidation;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\organizer\[actionname]
    /// 对Organizer的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OrganizerController : ApiControllerBase
    {
        public OrganizerController()
            : base()
        {

        }

        /// <summary>
        /// 获取当前登录Organizer所在的Organizations
        /// </summary>
        /// <returns></returns>
        [ActionName("be_in")]
        [HttpGet]
        public HttpResponseMessage BeInOrganizations()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/be_in") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return BeInOrganizations("Name", true, 0, 0);
        }
        [ActionName("be_in")]
        [HttpGet]
        public HttpResponseMessage BeInOrganizations(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/be_in?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            string json = "";
            if (organizer.UserRole.Contains(Role.Organizer))
            {
                List<object> Curs = new List<object>();
                foreach (var organization in myService.FindAllJoinedOrganizationByOrganizer(organizer, sortByKey, isAscending, pageIndex, pageSize))
                {
                    var Cur = new
                    {
                        name = organization.Name,
                        id = organization.Id,
                        time = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations[organization.Id],
                        avatar = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Avatar,
                        description = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Description
                    };
                    Curs.Add(Cur);
                }
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, Curs, Curs.GetType());
                json = tw.ToString();
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取当前登录Organizer可以加入的Organizations,即未加入也未申请的Organizations
        /// </summary>
        /// <returns></returns>
        [ActionName("to_join")]
        [HttpGet]
        public HttpResponseMessage ToJoinOrganization()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/to_join") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return ToJoinOrganization("Name", true, 0, 0);
        }
        [ActionName("to_join")]
        [HttpGet]
        public HttpResponseMessage ToJoinOrganization(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/to_join?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            string json = "";
            if (CurrentUser.UserRole.Contains(Role.Organizer))
            {
                List<object> Curs = new List<object>();
                foreach (var organization in myService.FindAllToJoinOrganizationByOrganizer(CurrentUser, sortByKey, isAscending, pageIndex, pageSize))
                {
                    var Cur = new
                    {
                        name = organization.Name,
                        id = organization.Id,
                        avatar = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Avatar,
                        description = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Description
                    };
                    Curs.Add(Cur);
                }
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, Curs, Curs.GetType());
                json = tw.ToString();
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取当前organizer已申请但还未接受或拒绝的organization
        /// </summary>
        /// <returns></returns>
        [ActionName("AppliedOrganization")]
        [HttpGet]
        public HttpResponseMessage AppliedOrganization()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/appliedorganization") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return AppliedOrganization("Name", true, 0, 0);
        }
        [ActionName("AppliedOrganization")]
        [HttpGet]
        public HttpResponseMessage AppliedOrganization(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/appliedorganization?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            List<object> Curs = new List<object>();
            string json = "";
            if (organizer.UserRole.Contains(Role.Organizer))
            {
                foreach (var organization in myService.FindAllAppliedOrganizationByOrganizer(organizer, sortByKey, isAscending, pageIndex, pageSize))
                {
                    var Cur = new
                    {
                        name = organization.Name,
                        id = organization.Id,
                        avatar = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Avatar,
                        description = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Description
                    };
                    Curs.Add(Cur);
                }
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, Curs, Curs.GetType());
                json = tw.ToString();
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 该organizer申请organization的历史记录
        /// </summary>
        /// <returns></returns>
        [ActionName("AppliedHistory")]
        [HttpGet]
        public HttpResponseMessage AppliedHistory()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/appliedhistory") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return AppliedHistory("actionTime", false, 0, 0);
        }
        //排序并分页
        [ActionName("AppliedHistory")]
        [HttpGet]
        public HttpResponseMessage AppliedHistory(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/appliedhistory?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            List<object> source = new List<object>();
            foreach (var information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                User organization = (User)myService.FindOneById(information.ApplyOrganizationId);
                source.Add(new AppliedHistoryModel
                {
                    organizationId = information.ApplyOrganizationId,
                    organizationName = information.ApplyOrganizationName,
                    applyTime = information.ApplyTime,
                    actionTime = information.ActionTime,
                    status = information.Status,
                    comment = information.Comment,
                    avatar = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).Avatar
                });
            }
            List<object> result = myService.SortAndPaging(source, sortByKey, isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 当前登录Organizer申请加入一个Organization
        /// </summary>
        /// <param name="model">所要加入organization的id</param>
        /// <returns>成功返回200，失败返回400、401</returns>
        [ActionName("join")]
        [HttpPost]
        public HttpResponseMessage JoinOrganization([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/join") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            User Organization = myService.FindUser(new Guid(model.id));
            if (Organization == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("Organization不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.OrganizerApplyToJoinOrganization(CurrentUser, Organization, model.comment))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 当前登录Organizer主动退出一个已加入的Organization，成功则分别给organization和organizer发送信息
        /// </summary>
        /// <param name="model">要退出organization的id</param>
        /// <returns></returns>
        [ActionName("quit")]
        [HttpPost]
        public HttpResponseMessage QuitOrganization([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/quit") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            User organization = myService.FindUser(new Guid(model.id));
            if (organization == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("Organization不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.OrganizerQuitOrganization(organizer, organization))
            {
                //myService.MessageService.SendMessage("System", organizer.Id, "你主动离开了组织", "你主动离开了组织" + organization.Name, null, null, false);
                //myService.MessageService.SendMessage("System", organization.Id, "有人离开了组织", organizer.Name + "离开了组织", null, null, false);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 获取所有该organizer能check in的所有activity
        /// </summary>
        /// <returns></returns>
        [ActionName("ActivityToCheckIn")]
        public HttpResponseMessage GetActivityToCheckIn()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activitytocheckin") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return GetActivityToCheckIn("Name", true, 0, 0);
        }
        [ActionName("ActivityToCheckIn")]
        public HttpResponseMessage GetActivityToCheckIn(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activitytocheckin?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            List<Activity> result = new List<Activity>();
            foreach (Activity activity in myService.FindActivatedActivitesByOrganizerId(CurrentUser.Id, "", sortByKey, isAscending, pageIndex, pageSize))
            {
                if ((activity.Status == ActivityStatus.RunningCheckIn) || (activity.Status == ActivityStatus.RunningSignInAndCheckIn))
                {
                    result.Add(activity);
                }
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取所有该organizer能check out的所有activity
        /// </summary>
        /// <returns></returns>
        [ActionName("ActivityToCheckOut")]
        public HttpResponseMessage GetActivityToCheckOut()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activitytocheckout") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return GetActivityToCheckOut("Name", true, 0, 0);
        }
        [ActionName("ActivityToCheckOut")]
        public HttpResponseMessage GetActivityToCheckOut(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activitytocheckout?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User CurrentUser = ValidationService.FindUserWithToken(GetToken());
            List<Activity> result = new List<Activity>();
            foreach (Activity activity in myService.FindActivatedActivitesByOrganizerId(CurrentUser.Id, "", sortByKey, isAscending, pageIndex, pageSize))
            {
                if (activity.Status == ActivityStatus.RunningCheckIn || activity.Status == ActivityStatus.Finished || activity.Status == ActivityStatus.RunningSignInAndCheckIn)
                {
                    result.Add(activity);
                }
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        /// <summary>
        /// （当前用户为organizer）在activity中check in某个volunteer
        /// </summary>
        /// <param name="checkInModel"></param>
        /// <returns></returns>
        [ActionName("CheckInActivity")]
        [HttpPost]
        public HttpResponseMessage CheckIn([FromBody]CheckInOrKickOutModel checkInModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/checkinactivity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(checkInModel.activityId));
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无check in权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<string> errorName = new List<string>();
            foreach (string volunteerId in checkInModel.volunteerIds)
            {
                User volunteer = myService.FindUser(new Guid(volunteerId));
                if (activity.CheckIn(volunteer))
                {
                    continue;
                }
                else
                {
                    errorName.Add(volunteer.Name);
                    continue;
                }
            }
            if (errorName.Any())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("以下用户状态错误，无法check in" + errorName.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// （当前用户为organizer）在activity中check out某个volunteer
        /// </summary>
        /// <param name="checkOutModel"></param>
        /// <returns></returns>
        [ActionName("CheckOutActivity")]
        [HttpPost]
        public HttpResponseMessage CheckOut([FromBody]CheckOutModel checkOutModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/checkoutactivity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(checkOutModel.activityId));
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无check out权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<string> errorName = new List<string>();
            foreach (string volunteerId in checkOutModel.volunteerIds)
            {
                User volunteer = myService.FindUser(new Guid(volunteerId));
                if (activity.CheckOut(volunteer, checkOutModel.isComplete))
                {
                    continue;
                }
                else
                {
                    errorName.Add(volunteer.Name);
                    continue;
                }
            }
            if (errorName.Any())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("以下用户状态错误，无法check out:" + errorName.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// （当前用户为organizer）在activity中直接checkout某个volunteer
        /// 该volunteer为处于signIn状态或者checkIn状态的
        /// 该web api为（如果处于signIn状态）先check in该volunteer，紧接着check out他
        /// </summary>
        /// <param name="checkOutModel"></param>
        /// <returns></returns>
        [ActionName("CheckOutDirectly")]
        [HttpPost]
        public HttpResponseMessage CheckOutDirectly([FromBody]CheckOutModel checkOutModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/checkoutdirectly") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(checkOutModel.activityId));
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无直接check out权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<string> errorName = new List<string>();
            foreach (string volunteerid in checkOutModel.volunteerIds)
            {
                User volunteer = myService.FindUser(new Guid(volunteerid));
                if (activity.CheckOutDirectly(volunteer, checkOutModel.isComplete))
                {
                    continue;
                }
                else
                {
                    errorName.Add(volunteer.Name);
                    continue;
                }
            }
            if (errorName.Any())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("以下用户状态错误，无法直接check out:" + errorName.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 将已经sign in的volunteer踢出活动，被踢Volunteer无法再次sign in
        /// </summary>
        /// <param name="kickOutModel"></param>
        /// <returns></returns>
        [ActionName("KickOut")]
        [HttpPost]
        public HttpResponseMessage KickVolunteerOut([FromBody]CheckInOrKickOutModel kickOutModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/kickout") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(kickOutModel.activityId));
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无踢人权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<string> errorName = new List<string>();
            foreach (string volunteerId in kickOutModel.volunteerIds)
            {
                User volunteer = myService.FindUser(new Guid(volunteerId));
                if (activity.KickVolunteerOut(volunteer))
                {
                    continue;
                }
                else
                {
                    errorName.Add(volunteer.Name);
                    continue;
                }
            }
            if (errorName.Any())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("以下用户状态错误，无法踢出" + errorName.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 误踢用户恢复
        /// </summary>
        /// <param name="unKickOutModel"></param>
        /// <returns></returns>
        [ActionName("UnKickOut")]
        [HttpPost]
        public HttpResponseMessage UnKickVolunteerOut([FromBody]CheckInOrKickOutModel unKickOutModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/unkickout") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(unKickOutModel.activityId));
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无误踢用户恢复权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<string> errorName = new List<string>();
            foreach (string volunteerId in unKickOutModel.volunteerIds)
            {
                User volunteer = myService.FindUser(new Guid(volunteerId));
                if (activity.UnKickVolunteerOut(volunteer))
                {
                    continue;
                }
                else
                {
                    errorName.Add(volunteer.Name);
                    continue;
                }
            }
            if (errorName.Any())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("以下用户状态错误，无法恢复" + errorName.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 生成一条的ActionValidation存数据库（只有该activity的创建人能够操作）
        /// 生成用户可以进行check in、check out等操作的二维码（二维码格式为：action, 该ActionValidation的id）
        /// </summary>
        /// <param name="activityIdModel"></param>
        /// <returns></returns>
        [ActionName("ActivityActionQRCode")]
        [HttpGet]
        public HttpResponseMessage ActivityActionQRCode(string activityId, string action)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activityactionqrcode?activityid=&action=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid id = new Guid(activityId);
            Activity activity = myService.FindActivity(id);
            User organizer = ValidationService.FindUserWithToken(GetToken());
            //判断权限
            if (myService.CheckIfOrganizerCanManageActivity(activity, organizer) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("登陆用户无check in权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //到期时间为当前时间之后 1 min 5 second
            DateTime expireTime = DateTime.Now + new TimeSpan(0, 1, 5);
            ActionValidationModel actionValidate;
            switch (action.ToLower())
            {
                case "checkin":
                    actionValidate = myService.ActionValidationService.GenerateActionValidate("CheckIn", id, expireTime);
                    break;
                case "checkoutnotcomplete":
                    actionValidate = myService.ActionValidationService.GenerateActionValidate("CheckOutNotComplete", id, expireTime);
                    break;
                case "checkoutcomplete":
                    actionValidate = myService.ActionValidationService.GenerateActionValidate("CheckOutComplete", id, expireTime);
                    break;
                case "checkoutdirectlynotcomplete":
                    actionValidate = myService.ActionValidationService.GenerateActionValidate("CheckOutDirectlyNotComplete", id, expireTime);
                    break;
                case "checkoutdirectlycomplete":
                    actionValidate = myService.ActionValidationService.GenerateActionValidate("CheckOutDirectlyComplete", id, expireTime);
                    break;
                default:
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("action错误，无法生成二维码！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            MemoryStream ms = new MemoryStream();
            myService.ActionValidationService.GenerateQRCode(action.ToLower() + "," + actionValidate.Id.ToString(), ms);
            byte[] buffer = ms.GetBuffer();
            string base64 = Convert.ToBase64String(buffer);
            var result = new
            {
                expireTime = expireTime,
                image = base64
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 生成一条的ActionValidation存数据库
        /// 生成发送短信的二维码（二维码格式为：activitysendsms, 该ActionValidation的id）
        /// </summary>
        /// <returns></returns>
        [ActionName("ActivitySendSMSQRCode")]
        [HttpPost]
        public HttpResponseMessage ActivitySendSMSQRCode([FromBody]GenerateSendSMSQRCodeModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/organizer/activitysendsmsqrcode") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //到期时间为当前时间之后 3 小时
            DateTime expireTime = DateTime.Now + new TimeSpan(3, 0, 0);
            ActionValidationModel actionValidate = myService.ActionValidationService.GenerateActionValidate("ActivitySendSMS", model, expireTime);
            MemoryStream ms = new MemoryStream();
            myService.ActionValidationService.GenerateQRCode("activitysendsms," + actionValidate.Id.ToString(), ms);
            byte[] buffer = ms.GetBuffer();
            string base64 = Convert.ToBase64String(buffer);
            var result = new
            {
                expireTime = expireTime,
                image = base64
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// organizer扫描发短信的二维码，获得发送人activityId、短信内容、发送目标的姓名和手机号码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("ActivitySendSMSQRCode")]
        [HttpGet]
        public HttpResponseMessage ActivitySendSMSQRCode(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/activitysendsmsqrcode?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //验证actionValidation(是否过期、action是否对应)
            if (!myService.ActionValidationService.Validate(id, "ActivitySendSMS"))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            ActionValidationModel actionValidation = myService.ActionValidationService.FindOneById(id);
            var result = actionValidation.Target;
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获取统计
        /// </summary>
        /// <returns></returns>
        [ActionName("Statistics")]
        [HttpGet]
        public HttpResponseMessage GetOrganizerStatistics()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/organizer/statistics") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organizer = ValidationService.FindUserWithToken(GetToken());
            IEnumerable<User> organizations = myService.FindAllJoinedOrganizationByOrganizer(organizer, "", true, 0, 0);
            var result = new List<object>();
            foreach (User organization in organizations)
            {
                var statistic = new
                {
                    OrganizationName = organization.Name,
                    TotalPointEachMonth = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).TotalPointEachMonth,
                    RemainingSum = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum,
                    ConsumeAllPoint = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ConsumeAllPoint,
                    ActivityCountLastMonth = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizationStatistics.LastOrDefault().ActivityCount,
                    VolunteerCountLastMonth = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizationStatistics.LastOrDefault().VolunteerCount,
                    AllActivityCount = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllActivityCount,
                    AllVolunteerCount = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllVolunteerCount,
                    AverageVolunteerEachActivity = (double)((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllVolunteerCount / ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).AllActivityCount,
                    HexagramProperty = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizationStatistics.LastOrDefault().HexagramProperty,
                };
                result.Add(statistic);
            }
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
        public class CheckInOrKickOutModel
        {
            public string activityId { get; set; }
            public List<string> volunteerIds { get; set; }
        }
        public class CheckOutModel
        {
            public string activityId { get; set; }
            public List<string> volunteerIds { get; set; }
            public bool isComplete { get; set; }
        }
        public class GenerateSendSMSQRCodeModel
        {
            public string activityId { get; set; }
            public string content { get; set; }
            public Dictionary<string, string> volunteerNameAndPhoneNumber { get; set; }
        }
        public class AppliedHistoryModel
        {
            public Guid organizationId { get; set; }
            public string organizationName { get; set; }
            public DateTime applyTime { get; set; }
            public DateTime actionTime { get; set; }
            public ApplyOrganizationStatus status { get; set; }
            public string comment { get; set; }
            public UserAvatar avatar { get; set; }
        }
    }
}