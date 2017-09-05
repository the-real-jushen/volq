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
using Jtext103.Volunteer.Friend;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using Jtext103.Volunteer.Badge;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\volunteer\[actionname]
    /// 对Volunteer的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public partial class VolunteerController : ApiControllerBase
    {
        public VolunteerController()
            : base()
        {

        }
        /// <summary>
        /// 返回用户浏览过的activity
        /// </summary>
        /// <returns></returns>
        [ActionName("ViewedActivities")]
        [HttpGet]
        public HttpResponseMessage MyViewedActivities(ActivityStage stage, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/viewedactivities?stage=&filterSource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            if (!currentUser.UserRole.Contains(Role.Volunteer))
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<Activity> activities = myService.FindAllActivitiesWhichVolunteerViewed(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize).ToList<Activity>();
            List<ActivityToListShow> result = transformActivityToListShow(activities, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 清空登录用户的activity浏览记录
        /// </summary>
        /// <returns></returns>
        [ActionName("ClearViewedActivityHistory")]
        [HttpPost]
        public HttpResponseMessage ClearViewActivityHistory()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/clearviewedactivityhistory") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            myService.ClearVolunteerViewActivityRecord(currentUser);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// volunteer加入一个Activity
        /// </summary>
        /// <param name="model">活动id</param>
        /// <returns></returns>
        [ActionName("SignInActivity")]
        [HttpPost]
        public HttpResponseMessage SignInActivity([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/signinactivity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = (Activity)myService.FindOneById(new Guid(model.id));
            if (activity == null || volunteer == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //判断volunteer badge是否满足条件
            if (myService.ActivityValidateBadgeLimit(activity, volunteer.Id) == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("volunteer的badge不符合要求，无法sign in", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (activity.SignIn(volunteer))
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            else return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("volunteer与activity状态错误，无法sign in", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// volunteer退出一个Activity
        /// </summary>
        /// <returns></returns>
        [ActionName("SignOutActivity")]
        [HttpPost]
        public HttpResponseMessage SignOutActivity([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/signoutactivity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            Activity activity = (Activity)myService.FindOneById(new Guid(model.id));
            if (activity == null || volunteer == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请求不合法", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            /*
            myService.VolunteerSignOutActivity(volunteer, activity);
            return new HttpResponseMessage(HttpStatusCode.OK);
            */
            if (activity.SignOut(volunteer))
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            else return new HttpResponseMessage { StatusCode = HttpStatusCode.NotAcceptable, Content = new StringContent("volunteer与activity状态错误，无法sign out", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }
                
        /// <summary>
        /// 返回当前登录用户收藏的activity
        /// </summary>
        /// <returns></returns>
        [ActionName("Favorite")]
        [HttpGet]
        public HttpResponseMessage MyFavoriteActivities(ActivityStage stage, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/favorite?stage=&filterSource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            if (!currentUser.UserRole.Contains(Role.Volunteer))
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            IEnumerable<Activity> source;
            switch (stage)
            {
                //找到所有非draft状态的activity
                case ActivityStage.all:
                    source = myService.FindAllActivitiesWhichVolunteerFavorite(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //即将开始的activity(处于active、maxVolunteer、ready、signIn状态下的)
                case ActivityStage.aboutToStart:
                    source = myService.FindAboutToStartActivitiesWhichVolunteerFavorite(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //正在进行的活动(处于RunningCheckIn、RunningRun状态下的)
                case ActivityStage.running:
                    source = myService.FindRunningActivitiesWhichVolunteerFavorite(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //已经完成的活动(处于Finished状态下的)
                case ActivityStage.finish:
                    source = myService.FindFinishedActivitiesWhichVolunteerFavorite(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                default:
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("stage参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<ActivityToListShow> result = transformActivityToListShow(source, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        /// <summary>
        /// 收藏一个activity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("Favorite")]
        [HttpPost]
        public HttpResponseMessage FavoriteActivity([FromBody]ActivityIdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/favorite") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(model.activityId);
            Activity activity = (Activity)myService.FindOneById(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            myService.VolunteerFavoriteActivity(currentUser, activity);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 取消收藏一个activity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("Unfavorite")]
        [HttpPost]
        public HttpResponseMessage UnfavoriteActivity([FromBody]ActivityIdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/unfavorite") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(model.activityId);
            Activity activity = (Activity)myService.FindOneById(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            myService.VolunteerUnFavoriteActivity(currentUser, activity);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 获得当前volunteer的point
        /// </summary>
        /// <returns></returns>
        [ActionName("Point")]
        [HttpGet]
        public HttpResponseMessage GetPoint()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/point") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            double point = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Point.TotalPoint;
            return new HttpResponseMessage { Content = new StringContent(point.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得某volunteer的统计数据
        /// </summary>
        /// <returns></returns>
        [ActionName("Statistics")]
        [HttpGet]
        public HttpResponseMessage GetMyStatistics(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/statistics?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = myService.FindUser(new Guid(id));
            //获得VolunteerProfile
            VolunteerProfile myProfile = (VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"];

            //计算参加活动总数和完成率
            int completedNumber = myService.FindAllVolunteerCompletedActivities(volunteer, "", false, 0, 0).Count();
            int totalNumber = myService.FindActivitesVolunteerSignedIn(volunteer, "", "", false, 0, 0).Count();
            double myCompleteRate;
            if (totalNumber != 0)
            {
                myCompleteRate = (double)completedNumber / totalNumber;
            }
            else
            {
                myCompleteRate = 0;
            }

            var result = new
            {
                //姓名
                name = volunteer.Name,
                id = volunteer.Id,
                //等级
                level = myProfile.VolunteerLevel,
                //等级名称
                levelName = myProfile.VolunteerLevelName,
                //等级对应图片
                levelPicture = myProfile.VolunteerLevelPicture,
                //总点数
                point = myProfile.Point.TotalPoint,
                //升级所需点数
                pointsToNextLevel = myProfile.PointsToNextLevel,
                //力量
                strength = myProfile.HexagramProperty.Strength,
                //智力
                intelligence = myProfile.HexagramProperty.Intelligence,
                //耐力
                endurance = myProfile.HexagramProperty.Endurance,
                //爱心
                compassion = myProfile.HexagramProperty.Compassion,
                //奉献
                sacrifice = myProfile.HexagramProperty.Sacrifice,
                //参加活动总数
                signedInActivityNumber = totalNumber,
                //完成率
                completeRate = myCompleteRate
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得我的总分排名及所有volunteer人数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("MyPointRank")]
        [HttpGet]
        public HttpResponseMessage GetMyPointRank(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/mypointrank?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var result = myService.MyPointRankOfAllVolunteer(new Guid(id));
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 修改用户方便活动的地点坐标信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Location")]
        [HttpPut]
        public HttpResponseMessage EditLocation([FromBody]LocationAndCoordinate model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/volunteer/location") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Location = model.location;
            ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Coordinate = model.coordinate;
            volunteer.Save();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 修改用户所属机构
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Affiliation")]
        [HttpPut]
        public HttpResponseMessage EditAffiliation([FromBody]AffiliationModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/volunteer/affiliation") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Affiliation = ParseToList(model.affiliations);
            myService.AffiliationService.AddTag(model.affiliations);
            volunteer.Save();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// 找到指定的ActionValidation，并执行相应的动作
        /// </summary>
        /// <param name="model">ActionValidation的id</param>
        /// <returns></returns>
        [ActionName("Action")]
        [HttpPost]
        public HttpResponseMessage ActionValidationAction([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/volunteer/action") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User volunteer = ValidationService.FindUserWithToken(GetToken());
            //先验证时间是否过期
            if (myService.ActionValidationService.Validate(model.id))
            {
                var actionValidation = myService.ActionValidationService.FindOneById(model.id);
                //action为CheckIn、CheckOut、CheckOutDirectly时，actionValidation.Target为目标activity的id
                if (actionValidation.Action == "CheckIn" || actionValidation.Action == "CheckOutComplete" || actionValidation.Action == "CheckOutNotComplete" || actionValidation.Action == "CheckOutDirectlyComplete" || actionValidation.Action == "CheckOutDirectlyNotComplete")
                {

                    Activity activity = myService.FindActivity((Guid)actionValidation.Target);
                    if (actionValidation.Action == "CheckIn")
                    {
                        if (activity.CheckIn(volunteer))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("check in失败！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                        }
                    }
                    else if (actionValidation.Action == "CheckOutComplete")
                    {
                        if (activity.CheckOut(volunteer, true))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("check out失败！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                        }
                    }
                    else if (actionValidation.Action == "CheckOutNotComplete")
                    {
                        if (activity.CheckOut(volunteer, false))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("check out失败！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                        }
                    }
                    else if (actionValidation.Action == "CheckOutDirectlyComplete")
                    {
                        if (activity.CheckOutDirectly(volunteer, true))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("直接check out失败！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                        }
                    }
                    else if (actionValidation.Action == "CheckOutDirectlyNotComplete")
                    {
                        if (activity.CheckOutDirectly(volunteer, false))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("直接check out失败！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("action错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                    }
                }
                //在这可以加入更多的action

                else
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("action错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                }
            }
            else
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("二维码已过期", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
        }

        /*
        /// <summary>
        /// 获得所有已签入的Activity
        /// </summary>
        /// <returns></returns>
        [ActionName("AllSignedInActivities")]
        [HttpGet]
        public HttpResponseMessage AllSignedInActivities()
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/allsignedactivities") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return AllSignedInActivities("StartTime", true, 0, 0);
        }
        public HttpResponseMessage AllSignedInActivities(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/volunteer/allsignedactivities?sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            List<Activity> activities = (List<Activity>)myService.FindAllVolunteerSignedInActivities(currentUser, sortByKey, isAscending, pageIndex, pageSize);
            List<ActivityToListShow> result = transformActivityToListShow(activities, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        */

        public class IdModel
        {
            public string id { get; set; }
        }
        
        public class LocationAndCoordinate
        {
            public string location { get; set; }
            public string coordinate { get; set; }
        }
        public class AffiliationModel
        {
            public string affiliations { get; set; }
        }

    }
}