using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.Friend;


namespace Jtext103.Volunteer.Web.Controllers
{
    public partial class VolunteerController : ApiControllerBase
    {
        /// <summary>
        /// 申请好友
        /// </summary>
        /// <param name="model">发送好友请求的volunteer的id和申请内容</param>
        /// <returns></returns>
        [ActionName("ApplyFriend")]
        [HttpPost]
        public HttpResponseMessage ApplyFriend([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/applyfriend") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid otherVolunteerId = new Guid(model.id);
            User myself = ValidationService.FindUserWithToken(GetToken());
            User other = myService.FindUser(otherVolunteerId);
            Guid myId = myself.Id;
            //无法和自己加好友
            if (myId == otherVolunteerId)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无法和自己加好友", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //检查是否已经是好友
            if (FriendService.CheckIfWeAreFriends(myId, otherVolunteerId) == true)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("你们已经是好友了", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //发送申请好友请求
            if (myService.FriendServiceInVolunteerService.ApplyFriend(myself, other, model.comment) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无法申请好友", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 同意一个volunteer的好友请求，并添加好友
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("AcceptFriend")]
        [HttpPost]
        public HttpResponseMessage AcceptFriend([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/acceptfriend") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid otherVolunteerId = new Guid(model.id);
            User myself = ValidationService.FindUserWithToken(GetToken());
            User other = myService.FindUser(otherVolunteerId);
            Guid myId = myself.Id;
            //检查是否已经是好友
            if (FriendService.CheckIfWeAreFriends(myId, otherVolunteerId) == true)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("你们已经是好友了", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //同意好友申请并添加好友
            if (myService.FriendServiceInVolunteerService.AcceptFriendApplication(other, myself, model.comment) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("同意好友申请不成功", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 拒绝一个volunteer的好友请求
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("RefuseFriend")]
        [HttpPost]
        public HttpResponseMessage RefuseFriend([FromBody]IdAndCommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/refusefriend") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid otherVolunteerId = new Guid(model.id);
            User myself = ValidationService.FindUserWithToken(GetToken());
            User other = myService.FindUser(otherVolunteerId);
            Guid myId = myself.Id;
            if (FriendService.CheckIfWeAreFriends(myId, otherVolunteerId) == true)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("你们已经是好友了", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //拒绝好友申请
            if (myService.FriendServiceInVolunteerService.RefuseFriendApplication(other, myself, model.comment) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("拒绝好友申请不成功", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 断绝好友关系
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("BreakOffFriendship")]
        [HttpPost]
        public HttpResponseMessage BreakOffFriendship([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/breakofffriendship") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid otherVolunteerId = new Guid(model.id);
            User myself = ValidationService.FindUserWithToken(GetToken());
            User other = myService.FindUser(otherVolunteerId);
            Guid myId = myself.Id;
            if (FriendService.CheckIfWeAreFriends(myId, otherVolunteerId) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("你们不是好友，无法断绝关系", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.FriendServiceInVolunteerService.BreakOffFriendship(myself, other) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("断绝好友关系不成功", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 为给定id的volunteer推荐指定个数的好友（如果指定个数过多，则返回除给定volunteer的所有volunteer）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        [ActionName("RecommendFriend")]
        [HttpGet]
        public HttpResponseMessage RecommendFriend(string id, int number)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/recommendfriend?id=&number=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = myService.FindUser(new Guid(id));
            var source = myService.FriendServiceInVolunteerService.RecommendFriends(volunteer, number);
            List<object> result = new List<object>();
            foreach (User recommend in source)
            {
                var a = new
                {
                    id = recommend.Id,
                    name = recommend.Name,
                    avatar = ((VolunteerProfile)recommend.UserProfiles[recommend.Name + "VolunteerProfile"]).Avatar,
                    description = ((VolunteerProfile)recommend.UserProfiles[recommend.Name + "VolunteerProfile"]).Description
                };
                result.Add(a);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 找到给定id的volunteer的所有好友
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("MyFriends")]
        [HttpGet]
        public HttpResponseMessage GetMyFriends(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/myfriends?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid myId = new Guid(id);
            List<object> result = new List<object>();
            IEnumerable<User> myFriendsOrderByPoint = myService.FriendServiceInVolunteerService.MyFriends(myId, "", false, 0, 0);
            foreach (User volunteer in myFriendsOrderByPoint)
            {
                var a = new
                {
                    id = volunteer.Id,
                    name = volunteer.Name,
                    avatar = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Avatar,
                    description = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Description,
                    level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel,
                };
                result.Add(a);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 我的好友总数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("MyFriendCount")]
        [HttpGet]
        public HttpResponseMessage GetMyFriendCount(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/myfriendcount?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid myId = new Guid(id);
            int result = FriendService.FindMyFriends(myId).Count;
            return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 对自己和所有好友进行排名，并排序分页
        /// sortByKey参数只能为"","point","activityCount","badgeCount"中的一个，否则返回404 NotFound
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("MyFriendsRank")]
        [HttpGet]
        public HttpResponseMessage GetMyFriendsRank(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/myfriendsrank?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid myId = new Guid(id);
            List<VolunteerAndFriendsRankModel> result = new List<VolunteerAndFriendsRankModel>();
            if (sortByKey == "" || sortByKey == "point" || sortByKey == "activityCount" || sortByKey == "badgeCount")
            {
                result = myService.FriendServiceInVolunteerService.MeAndMyFriendsRank(myId, sortByKey, isAscending, pageIndex, pageSize);
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("sortByKey参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }



        /// <summary>
        /// 获得我的排名
        /// sortByKey参数只能为"","point","activityCount","badgeCount"中的一个，否则返回404 NotFound
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        [ActionName("MyRank")]
        [HttpGet]
        public HttpResponseMessage GetMyRank(string id, string sortByKey, bool isAscending)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/myrank?id=&sortByKey=&isAscending=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid myId = new Guid(id);
            if (sortByKey == "" || sortByKey == "point" || sortByKey == "activityCount" || sortByKey == "badgeCount")
            {
                int result = myService.FriendServiceInVolunteerService.GetMyRank(myId, sortByKey, isAscending);
                return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("sortByKey参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
        }

        /// <summary>
        /// 所有(我还未处理的)向我申请好友的记录
        /// </summary>
        [ActionName("ApplyToMeHistory")]
        [HttpGet]
        public HttpResponseMessage ApplyToMeHistory()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/applytomehistory") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return ApplyToMeHistory("Time", false, 0, 0);
        }
        //排序并分页
        [ActionName("ApplyToMeHistory")]
        [HttpGet]
        public HttpResponseMessage ApplyToMeHistory(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/applytomehistory?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            List<object> source = new List<object>();
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if (information.hasHandled == false)
                {
                    User friend = (User)myService.FindOneById(information.VolunteerId);
                    source.Add(new ApplyToMeModel
                    {
                        id = information.VolunteerId,
                        name = information.Name,
                        avatar = ((VolunteerProfile)friend.UserProfiles[friend.Name + "VolunteerProfile"]).Avatar,
                        Comment = information.Comment,
                        Time = information.Time,
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
        /// 所有我向其他人申请好友的记录
        /// </summary>
        [ActionName("ApplyFromMeHistory")]
        [HttpGet]
        public HttpResponseMessage ApplyFromMeHistory()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/applyfrommehistory") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return ApplyFromMeHistory("ActionTime", false, 0, 0);
        }
        //排序并分页
        [ActionName("ApplyFromMeHistory")]
        [HttpGet]
        public HttpResponseMessage ApplyFromMeHistory(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/applyfrommehistory?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            List<object> source = new List<object>();
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                User friend = (User)myService.FindOneById(information.VolunteerId);
                source.Add(new ApplyFromMeModel
                {
                    id = information.VolunteerId,
                    Name = information.Name,
                    avatar = ((VolunteerProfile)friend.UserProfiles[friend.Name + "VolunteerProfile"]).Avatar,
                    Comment = information.Comment,
                    ApplyTime = information.ApplyTime,
                    ActionTime = information.ActionTime,
                    Status = information.Status
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
        /// 通过email、名字、从属 找到我的好友
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [ActionName("SearchMyFriendByFilter")]
        [HttpGet]
        public HttpResponseMessage SearchMyFriendByFilter(string email, string friendName, string affiliation)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/searchmyfriendbyfilter?email=&friendname=&affiliation=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User me = ValidationService.FindUserWithToken(GetToken());
            var source = myService.FriendServiceInVolunteerService.SearchMyFriendByFilter(me.Id, email, friendName, affiliation);
            List<object> result = new List<object>();
            foreach (User volunteer in source)
            {
                var a = new
                {
                    id = volunteer.Id,
                    name = volunteer.Name,
                    avatar = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Avatar,
                    description = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Description,
                    level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel
                };
                result.Add(a);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 通过email、名字、从属 找到还不是我的好友的volunteer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [ActionName("SearchNotMyFriendByFilter")]
        [HttpGet]
        public HttpResponseMessage SearchNotMyFriendByFilter(string email, string friendName, string affiliation)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/searchnotmyfriendbyfilter?email=&friendname=&affiliation=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User me = ValidationService.FindUserWithToken(GetToken());
            var source = myService.FriendServiceInVolunteerService.SearchNotMyFriendByFilter(me.Id, email, friendName, affiliation);
            List<object> result = new List<object>();
            foreach (User volunteer in source)
            {
                var a = new
                {
                    id = volunteer.Id,
                    name = volunteer.Name,
                    avatar = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Avatar,
                    description = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Description,
                    level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel
                };
                result.Add(a);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 判断某一个user是否为我的好友
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("IsMyFriend")]
        [HttpGet]
        public HttpResponseMessage IsMyFriend(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/ismyfriend?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User me = ValidationService.FindUserWithToken(GetToken());
            IEnumerable<User> myFriends = myService.FriendServiceInVolunteerService.MyFriends(me.Id, "", false, 0, 0);
            int result = 0;
            foreach (User friend in myFriends)
            {
                if (friend.Id.Equals(new Guid(id)))
                {
                    result = 1;
                }
            }
            return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }
        /// <summary>
        /// 判断某一个user是否向我申请成为好友且还未处理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("HasAppliedToMe")]
        [HttpGet]
        public HttpResponseMessage HasAppliedToMe(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/hasappliedtome?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User me = ValidationService.FindUserWithToken(GetToken());
            int result = 0;
            foreach (ApplyFriendToMe information in ((VolunteerProfile)me.UserProfiles[me.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if (information.hasHandled == false && information.VolunteerId.Equals(new Guid(id)))
                {
                    result = 1;
                    break;
                }
            }
            return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }
    }
    public class IdAndCommentModel
    {
        public string id { get; set; }
        public string comment { get; set; }
    }
    public class ApplyFromMeModel
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public UserAvatar avatar { get; set; }
        public DateTime ApplyTime { get; set; }
        public DateTime ActionTime { get; set; }
        public string Comment { get; set; }
        public ApplyFriendStatus Status { get; set; }
    }
    public class ApplyToMeModel
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public UserAvatar avatar { get; set; }
        public DateTime Time { get; set; }
        public string Comment { get; set; }
    }
}