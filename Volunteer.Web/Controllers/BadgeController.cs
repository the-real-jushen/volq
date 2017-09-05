using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Jtext103.Volunteer.Service;
using Jtext103.Volunteer.Badge;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Volunteer.Friend;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\badge\[actionname]
    /// 对badge的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BadgeController : ApiControllerBase
    {
        public BadgeController()
            : base()
        {

        }

        /// <summary>
        /// 获得所有的badge name
        /// </summary>
        /// <returns></returns>
        [ActionName("AllBadges")]
        [HttpGet]
        public HttpResponseMessage GetAllBadges(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/badge/allbadges?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<BadgeDescription> result = BadgeService.FindAllBadges(sortByKey, isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得所有badge的个数
        /// </summary>
        /// <returns></returns>
        [ActionName("AllBadgeCount")]
        [HttpGet]
        public HttpResponseMessage GetAllBadgeCount()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/badge/allbadgecount") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var result = BadgeService.FindAllBadgeCount();
            return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 获得该用户所有已获得的badge name
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [ActionName("UserBadges")]
        [HttpGet]
        public HttpResponseMessage GetUserBadges(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/badge/userbadges?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid userId = new Guid(id);
            User user = myService.FindUser(userId);
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            //如果当前用户和user都是volunteer，必须是自己或者好友才能调用该web api看到badge
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
            List<BadgeEntity> source = BadgeService.FindAllUserGrantedBadgeEntity(userId, sortByKey, isAscending, pageIndex, pageSize);
            var result = transformBadgeEntityToListShow(source);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得该用户所有已获得的badge的个数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("UserBadgeCount")]
        [HttpGet]
        public HttpResponseMessage GetUserBadgeCount(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/badge/userbadgecount?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var result = BadgeService.FindAllUserGrantedBadgeCount(new Guid(id));
            return new HttpResponseMessage { Content = new StringContent(result.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 获得badge详情
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="badgeName">badge的name</param>
        /// <returns></returns>
        [ActionName("UserBadgeDetail")]
        [HttpGet]
        public HttpResponseMessage GetUserBadgeDetail(string id, string badgeName)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/badge/userbadgedetail?id=&badgename=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<BadgeEntity> badgeEntities = BadgeService.FindAllUserGrantedBadgeEntity(new Guid(id));
            foreach (BadgeEntity badgeEntity in badgeEntities)
            {
                if (badgeEntity.BadgeName == badgeName)
                {
                    BadgeDescription badgeDescription = BadgeService.FindBadgeDescriptionByName(badgeEntity.BadgeName);
                    var result = new
                    {
                        badgeName = badgeEntity.BadgeName,
                        badgeDescription = badgeDescription.Description,
                        badgePicture = badgeDescription.Picture,
                        badgeGrantedTime = badgeEntity.GrantedTime,
                        badgeRequirementDescription = badgeDescription.RequirementDescription.Values
                    };
                    StringWriter tw = new StringWriter();
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(tw, result, result.GetType());
                    return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
                }
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("未找到该badge", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 将BadgeEntity集合转换成用于列表显示
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<object> transformBadgeEntityToListShow(IEnumerable<BadgeEntity> source)
        {
            List<object> result = new List<object>();
            foreach (BadgeEntity badgeEntity in source)
            {
                BadgeDescription badgeDescription = BadgeService.FindBadgeDescriptionByName(badgeEntity.BadgeName);
                var listShow = new
                {
                    badgeName = badgeEntity.BadgeName,
                    badgeDescription = badgeDescription.Description,
                    badgePicture = badgeDescription.Picture
                };
                result.Add(listShow);
            }
            return result;
        }
    }
}