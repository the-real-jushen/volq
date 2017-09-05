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
using Newtonsoft.Json;
using System.Web.Http.Cors;
using System.Web;
using Jtext103.Volunteer.Friend;
using Jtext103.BlogSystem;
using Jtext103.ImageHandler;

namespace Jtext103.Volunteer.Web.Controllers
{
    /// <summary>
    /// url:\api\activity\[actionname]
    /// 对Activity的相关操作
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ActivityController : ApiControllerBase
    {
        public ActivityController()
            : base()
        {

        }
        ///// <summary>
        ///// 上传活动图片
        ///// </summary>
        ///// <returns></returns>
        //[ActionName("UpLoadImage")]
        //[HttpPost]
        //public HttpResponseMessage UpLoadActivityImage()
        //{
        //    if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/uploadimage") == false)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    var httpRequest = HttpContext.Current.Request;
        //    if (httpRequest.Files.Count != 1)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("error", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    var postedFile = httpRequest.Files[0];
        //    string imageName = Guid.NewGuid().ToString();//生成图像名称
        //    string path = "/Static/Images/Activity/" + imageName + postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("."));//相对路径+图像名称+图像格式
        //    string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径
        //    postedFile.SaveAs(filePath);//保存图像到本地
        //    return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new StringContent(path, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //}

        /// <summary>
        /// 生成activity活动详情页面网址的二维码
        /// </summary>
        /// <param name="model">activity的id</param>
        /// <returns></returns>
        [ActionName("QRCode")]
        [HttpPost]
        public HttpResponseMessage QRCode([FromBody]IdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/qrcode") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Activity activity = myService.FindActivity(new Guid(model.id));
            MemoryStream ms = new MemoryStream();
            myService.ActionValidationService.GenerateQRCode(@"http://www.volq.org/views/activity.html?id=" + model.id.ToString(), ms);
            byte[] buffer = ms.GetBuffer();
            string result = Convert.ToBase64String(buffer);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 由Organizer添加一个activity
        /// </summary>
        /// <param name="model">ActivityModel</param>
        /// <returns>成功返回202，失败返回401</returns>
        [ActionName("All")]
        [HttpPost]
        public HttpResponseMessage AddActivity([FromBody]CreateActivityModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }

            User organizer = ValidationService.FindUserWithToken(GetToken());
            User organization = myService.FindUser(new Guid(model.organizationId));

            Activity newActivity = new Activity()
            {
                OrganizerId = organizer.Id,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                Name = model.name,
                Abstract = model.activityAbstract,
                StartTime = DateTime.Parse(model.startTime, System.Globalization.CultureInfo.CurrentCulture),
                FinishTime = DateTime.Parse(model.finishTime, System.Globalization.CultureInfo.CurrentCulture),
                Point = model.point,
                LeastVolunteers = model.leastVolunteers,
                MostVolunteers = model.mostVolunteers,
                OpenSignInTime = DateTime.Parse(model.openSignInTime, System.Globalization.CultureInfo.CurrentCulture),
                CloseSignInTime = DateTime.Parse(model.closeSignInTime, System.Globalization.CultureInfo.CurrentCulture),
                Procedure = model.procedure == null ? "无" : model.procedure,
                Location = model.location == null ? "无" : model.location,
                Coordinate = model.coordinate == null ? "无" : model.coordinate,
                Requirement = model.requirement == null ? "无" : model.requirement,
                Photos = model.photos == null ? new List<string>() : model.photos,
                Videos = model.videos == null ? new List<string>() : model.videos,
                Tags = model.activitytags == null ? new List<string>() : ParseToList(model.activitytags),
                BadgeLimit = new ActivityBadgeLimit { MustGranted = ParseToListList(model.mustGranted), CantGranted = ParseToList(model.cantGranted) },
                HexagramProperty = new HexagramProperty(model.Strength, model.Intelligence, model.Endurance, model.Compassion, model.Sacrifice)
            };
            if (!newActivity.ValidateTime())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("activity开放注册时间、关闭注册时间、活动开始时间、活动结束时间非法！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (!newActivity.ValidateHexagram())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("六芒星属性值总和超过上限！上限为" + newActivity.HexagramPropertyTotalPointLimit, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum < newActivity.Point)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("organization剩余点数不足", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //设置活动封面
            newActivity.Cover = setActivityCover(newActivity.Photos);
            //activity存数据库
            myService.InsertOne(newActivity);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// （organizer）更新一个activity
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("All")]
        [HttpPut]
        public HttpResponseMessage UpdateActivity([FromBody]UpdateActivityModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "put:/api/activity") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(model.activityId));
            List<string> oldActivityPhotos = activity.Photos;
            User organization = myService.FindUser(new Guid(model.organizationId));
            if (activity.OrganizerId != currentUser.Id)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有创建该activity的organizer才能激活该activity", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (!((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(currentUser.Id))
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("当前Organizer不在activity所属organization中，无法修改", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (activity.Status != ActivityStatus.Draft)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("activity不在draft状态", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //修改activity
            else
            {
                activity.Name = model.name;
                activity.OrganizationId = organization.Id;
                activity.OrganizationName = organization.Name;
                activity.Abstract = model.activityAbstract;
                activity.StartTime = DateTime.Parse(model.startTime, System.Globalization.CultureInfo.CurrentCulture);
                activity.FinishTime = DateTime.Parse(model.finishTime, System.Globalization.CultureInfo.CurrentCulture);
                activity.Point = model.point;
                activity.LeastVolunteers = model.leastVolunteers;
                activity.MostVolunteers = model.mostVolunteers;
                activity.OpenSignInTime = DateTime.Parse(model.openSignInTime, System.Globalization.CultureInfo.CurrentCulture);
                activity.CloseSignInTime = DateTime.Parse(model.closeSignInTime, System.Globalization.CultureInfo.CurrentCulture);
                activity.Procedure = model.procedure == null ? activity.Procedure : model.procedure;
                activity.Location = model.location == null ? activity.Location : model.location;
                activity.Coordinate = model.coordinate == null ? activity.Coordinate : model.coordinate;
                activity.Requirement = model.requirement == null ? activity.Requirement : model.requirement;
                activity.Photos = model.photos == null ? activity.Photos : model.photos;
                activity.Videos = model.videos == null ? activity.Videos : model.videos;
                activity.Tags = model.activitytags == null ? activity.Tags : ParseToList(model.activitytags);
                activity.BadgeLimit = new ActivityBadgeLimit { MustGranted = ParseToListList(model.mustGranted), CantGranted = ParseToList(model.cantGranted) };
                activity.HexagramProperty = new HexagramProperty(model.Strength, model.Intelligence, model.Endurance, model.Compassion, model.Sacrifice);
            }
            if (!activity.ValidateTime())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("修改后的activity开放注册时间、关闭注册时间、活动开始时间、活动结束时间非法！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (!activity.ValidateHexagram())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("六芒星属性值总和超过上限！上限为" + activity.HexagramPropertyTotalPointLimit, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum < activity.Point)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("organization剩余点数不足", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (oldActivityPhotos.FirstOrDefault() != activity.Photos.FirstOrDefault())
            {
                //重新设置活动封面
                activity.Cover = setActivityCover(activity.Photos);
            }
            activity.Save();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        //设置活动封面
        //找到活动中第一张图，裁剪为270*200保存为封面，并保存到本地
        //如果活动没有图，则直接返回default.jpg
        private string setActivityCover(List<string> photos)
        {
            string result;
            if (photos == null)
            {
                result = "/Static/Images/Activity/default.jpg";
                return result;
            }
            if (photos.Count == 0)
            {
                result = "/Static/Images/Activity/default.jpg";
                return result;
            }
            try
            {
                string firstPhotoPath = HttpContext.Current.Server.MapPath("~" + photos.FirstOrDefault());
                Stream fileStream = new FileStream(firstPhotoPath, FileMode.Open);
                string imageName = Guid.NewGuid().ToString();//生成图像名称
                string path = "/Static/Images/Activity/" + imageName + ".jpg";//相对路径+图像名称+图像格式
                string filePath = HttpContext.Current.Server.MapPath("~" + path);//绝对路径
                result = path;
                //保存为270*200jpg图片
                HandleImageService.CutForCustom(fileStream, filePath, 270, 200, 75);
                return result;
            }
            catch
            {
                result = "/Static/Images/Activity/default.jpg";
                return result;
            }
        }

        /// <summary>
        /// （organization）激活一个draft状态下的activity
        /// </summary>
        /// <param name="activityIdModel"></param>
        /// <returns></returns>
        [ActionName("Active")]
        [HttpPost]
        public HttpResponseMessage ActivateDraftActivity([FromBody]ActivityIdModel activityIdModel)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/active") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User organization = ValidationService.FindUserWithToken(GetToken());
            Activity activity = myService.FindActivity(new Guid(activityIdModel.activityId));
            //User organization = myService.FindUser(activity.OrganizationId);
            //if (activity.OrganizerId != currentUser.Id)
            //{
            //    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有创建该activity的organizer才能激活该activity", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            //}
            //if (!((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(currentUser.Id))
            //{
            //    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("当前Organizer不在activity所属organization中，无法修改", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            //}
            if (activity.OrganizationId != organization.Id)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有该activity所属的organization才能激活该activity", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (activity.Status != ActivityStatus.Draft)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("activity不在draft状态", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (!activity.ValidateTime())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("activity开放注册时间、关闭注册时间、活动开始时间、活动结束时间非法！", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (!activity.ValidateHexagram())
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("六芒星属性值总和超过上限！上限为" + activity.HexagramPropertyTotalPointLimit, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum < activity.Point)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("organization剩余点数不足", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            activity.ActivateActivity();
            //如果该activity的tag如果不在tag pool中则存入tag pool，否则频率+1
            myService.ActivityTagService.AddTag(activity.Tags);
            //扣除点数
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum -= activity.Point;
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ConsumeAllPoint += activity.Point;
            organization.Save();
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 找到当前用户的名下的draft状态的活动
        /// 当为organizer时，返回该organizer创建的所有draft状态的活动
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("MineDraft")]
        [HttpGet]
        public HttpResponseMessage GetMineDraftActivities(string id, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/minedraft?id=&filterSource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = myService.FindUser(new Guid(id));
            List<Activity> source = new List<Activity>();
            foreach (Role role in currentUser.UserRole)
            {
                switch (role)
                {
                    case Role.Organizer:
                        source = myService.FindDraftActivitiesByOrganizerOrOrganization(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                        break;
                    case Role.Organization:
                        source = myService.FindDraftActivitiesByOrganizerOrOrganization(currentUser, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                        break;
                    case Role.Volunteer:
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("用户角色不符", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                    case Role.Anonymous:
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("用户角色不符", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
                    default:
                        break;
                }
                //目前user只有一个角色
                break;
            }
            List<ActivityToListShow> result = transformActivityToListShow(source, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        
        /// <summary>
        /// 返回某用户相关的activity
        /// volunteer：返回已sign in的activity
        /// organizer：返回该organizer创建的activity
        /// organization：返回该组织名下的activity
        /// </summary>
        /// <returns></returns>
        [ActionName("Mine")]
        public HttpResponseMessage GetMyActivity(string id, ActivityStage stage, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/mine?id=&stage=&filtersource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid userId = new Guid(id);
            User user = myService.FindUser(userId);
            //如果当前用户和user都是volunteer，必须是自己或者好友才能调用该web api看到活动
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
            IEnumerable<Activity> source;
            switch (stage)
            {
                //找到所有非draft状态的activity
                case ActivityStage.all:
                    source = myService.FindAllNotDraftActivities(user, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //即将开始的activity(处于active、maxVolunteer、ready、signIn状态下的)
                case ActivityStage.aboutToStart:
                    source = myService.FindAllAboutToStartActivities(user, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //正在进行的活动(处于RunningCheckIn、RunningRun状态下的)
                case ActivityStage.running:
                    source = myService.FindAllRunningActivities(user, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //已经完成的活动(处于Finished状态下的)
                case ActivityStage.finish:
                    source = myService.FindAllFinishedActivities(user, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                default:
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("stage参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<ActivityToListShow> result = transformActivityToListShow(source, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 返回所有非draft的Activity
        /// </summary>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetAll(ActivityStage stage, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity?stage=&filterSource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            IEnumerable<Activity> source;
            switch (stage)
            {
                //找到所有非draft状态的activity
                case ActivityStage.all:
                    source= myService.FindAllNotDraftActivities(null, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //即将开始的activity(处于active、maxVolunteer、ready、signIn状态下的)
                case ActivityStage.aboutToStart:
                    source = myService.FindAllAboutToStartActivities(null, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //正在进行的活动(处于RunningCheckIn、RunningRun状态下的)
                case ActivityStage.running:                    
                    source = myService.FindAllRunningActivities(null, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                //已经完成的活动(处于Finished状态下的)
                case ActivityStage.finish:
                    source = myService.FindAllFinishedActivities(null, filterSource, sortByKey, isAscending, pageIndex, pageSize);
                    break;
                default:
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("stage参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<ActivityToListShow> result = transformActivityToListShow(source, currentUser);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        
        /// <summary>
        /// 返回该id的Activity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("All")]
        public HttpResponseMessage GetActivityById(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //bool breakLoop = false;
            //foreach (Role role in currentUser.UserRole)
            //{
            //    if (role == Role.Organization || role == Role.Organizer)
            //    {
            //        if (activity.OrganizerId == currentUser.Id || activity.OrganizationId == currentUser.Id)
            //        {
            //            breakLoop = true;
            //            break;
            //        }
            //    }
            //    if (breakLoop)
            //    {
            //        break;
            //    }
            //    VolunteerParticipateInActivityRecord volunteerRecord = null;
            //    if (role == Role.Volunteer)
            //    {
            //        myService.VolunteerViewActivity(currentUser, activity);
            //        foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
            //        {
            //            if (record.VolunteerId == currentUser.Id)
            //            {
            //                volunteerRecord = record;
            //                breakLoop = true;
            //                break;
            //            }
            //        }
            //    }
            //    activity.VolunteerStatus.Clear();
            //    if (breakLoop)
            //    {
            //        activity.VolunteerStatus.Add(volunteerRecord);
            //        break;
            //    }
            //}
            VolunteerParticipateInActivityRecord myRecord = null;//当前用户为参与活动的volunteer时，对其赋值
            foreach (Role role in currentUser.UserRole)
            {
                if (role == Role.Volunteer)
                {
                    myService.VolunteerViewActivity(currentUser, activity);
                    foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
                    {
                        if (record.VolunteerId == currentUser.Id)
                        {
                            myRecord = record;
                            break;
                        }
                    }
                }
            }
            User organization = (User)myService.FindOneById(activity.OrganizationId);
            var result = new
            {
                Id = activity.Id,
                OrganizerId = activity.OrganizerId,
                OrganizationId = activity.OrganizationId,
                OrganizationName = activity.OrganizationName,
                OrganizationAvatar = organization.UserProfiles[organization.Name + "OrganizationProfile"].Avatar.AvatarPath,
                Name = activity.Name,
                Abstract = activity.Abstract,
                ActivateTime = activity.ActivateTime,
                OpenSignInTime = activity.OpenSignInTime,
                CloseSignInTime = activity.CloseSignInTime,
                StartTime = activity.StartTime,
                FinishTime = activity.FinishTime,
                Point = activity.Point,
                MostVolunteers = activity.MostVolunteers,
                LeastVolunteers = activity.LeastVolunteers,
                Location = activity.Location,
                Coordinate = activity.Coordinate,
                Procedure = activity.Procedure,
                Photos = activity.Photos,
                Videos = activity.Videos,
                Status = activity.Status,
                Tags = activity.Tags,
                BadgeLimit = activity.BadgeLimit,
                HexagramPropertyTotalPointLimit = activity.HexagramPropertyTotalPointLimit,
                HexagramProperty = activity.HexagramProperty,
                Requirement = activity.Requirement,
                VolunteerViewedTime = activity.VolunteerViewedTime,
                VolunteerFavoritedTime = activity.VolunteerFavoritedTime,
                HasSignedInVolunteerNumber = activity.HasSignedInVolunteerNumber,
                Rating = activity.Rating,
                VolunteersRecord = transformVolunteerRecordToListShow(activity.VolunteerStatus),
                MyRecord = myRecord,
                hasFavorited = myService.CheckIfVolunteerFavoriteActivity(currentUser, activity.Id),
                hasSignined = myService.CheckIfVolunteerSignInActivity(currentUser, activity.Id),
                hasViewed = myService.CheckIfVolunteerViewActivity(currentUser, activity.Id),
                myRate = activity.GetMyRate(currentUser.Id),
                ratedNumber = activity.RatedNumber,
                Cover = activity.Cover
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            return new HttpResponseMessage { Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得Organization名下的activity列表（含draft和非dtaft）
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [ActionName("Organization")]
        [HttpGet]
        public HttpResponseMessage GetOrganizationActivity(string organizationId, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/organization?organizationid=&filterSource=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid id = new Guid(organizationId);
            User organization = myService.FindUser(id);
            List<Activity> activities = myService.FindActivitesByOrganizationId(id, filterSource, sortByKey, isAscending, pageIndex, pageSize);
            List<ActivityToListShow> result = transformActivityToListShow(activities, organization);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string jsonString = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(jsonString, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 返回该Activity是否被登录用户收藏
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("IsFavorited")]
        public HttpResponseMessage IsFavorited(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/isfavorited?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(id);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(myService.CheckIfVolunteerFavoriteActivity(currentUser, activityId).ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// 返回该Activity是否被登录用户Signin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("IsSignedIn")]
        public HttpResponseMessage IsSignedIn(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/issignedin?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(id);
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(myService.CheckIfVolunteerSignInActivity(currentUser, activityId).ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        /// <summary>
        /// （当前用户为organizer）获取所有参加该id的activity的volunteers
        /// </summary>
        /// <param name="id">activity id</param>
        /// <returns>volunteer的id、name、在该活动中状态</returns>
        [ActionName("AllVolunteers")]
        public HttpResponseMessage GetAllVolunteersInActivity(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/allvolunteers?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return GetAllVolunteersInActivity(id, "", true, 0, 0);
        }
        [ActionName("AllVolunteers")]
        public HttpResponseMessage GetAllVolunteersInActivity(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/allvolunteers?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            bool isVolunteer = true;
            foreach (Role role in currentUser.UserRole)
            {
                if (role == Role.Organization || role == Role.Organizer)
                {
                    if (activity.OrganizerId == currentUser.Id || activity.OrganizationId == currentUser.Id)
                    {
                        isVolunteer = false;
                        break;
                    }
                }
            }
            if (isVolunteer == true)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("目前登陆身份无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //返回结果
            List<object> source = new List<object>();
            foreach (User volunteer in myService.FindAllVolunteerInActivity(activity, sortByKey, isAscending, 0, 0))
            {
                //该volunteer在activity中的状态
                VolunteerStatusInActivity volunteerStatus = VolunteerStatusInActivity.error;
                foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
                {
                    if (record.VolunteerId == volunteer.Id)
                    {
                        volunteerStatus = record.VolunteerStatus;
                        break;
                    }
                }
                var volunteerRecord = new
                {
                    VolunteerId = volunteer.Id,
                    VolunteerName = volunteer.Name,
                    VolunteerStatus = volunteerStatus
                };
                source.Add(volunteerRecord);
            }
            List<object> result = new List<object>();
            result = myService.SortAndPaging(source, "", isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得所有能够在该activity中check in的volunteer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("VolunteersToCheckIn")]
        [HttpGet]
        public HttpResponseMessage GetAllToCheckInVolunteersInActivity(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/volunteerstocheckin?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<object> source = new List<object>();
            foreach (User volunteer in myService.FindAllToCheckInVolunteersInActivity(activity, sortByKey, isAscending, 0, 0))
            {
                var volunteerRecord = new
                {
                    VolunteerId = volunteer.Id,
                    VolunteerName = volunteer.Name,
                };
                source.Add(volunteerRecord);
            }
            List<object> result = new List<object>();
            result = myService.SortAndPaging(source, "", isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得所有能够在该activity中check out的volunteer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("VolunteersToCheckOut")]
        [HttpGet]
        public HttpResponseMessage GetAllToCheckOutVolunteersInActivity(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/volunteerstocheckout?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<object> source = new List<object>();
            foreach (User volunteer in myService.FindAllToCheckOutVolunteersInActivity(activity, sortByKey, isAscending, 0, 0))
            {
                var volunteerRecord = new
                {
                    VolunteerId = volunteer.Id,
                    VolunteerName = volunteer.Name,
                };
                source.Add(volunteerRecord);
            }
            List<object> result = new List<object>();
            result = myService.SortAndPaging(source, "", isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得所有已经完成该activity的volunteer（已经checkout过，且该volunteer处于complete状态）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("CompletedVolunteers")]
        [HttpGet]
        public HttpResponseMessage GetAllCompletedVolunteersInActivity(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/completedvolunteers?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<object> source = new List<object>();
            foreach (User volunteer in myService.FindAllCompletedVolunteersInActivity(activity, sortByKey, isAscending, 0, 0))
            {
                var volunteerRecord = new
                {
                    VolunteerId = volunteer.Id,
                    VolunteerName = volunteer.Name,
                };
                source.Add(volunteerRecord);
            }
            List<object> result = new List<object>();
            result = myService.SortAndPaging(source, "", isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得所有已经无法完成该activity的volunteer（已经checkout过，且该volunteer处于notParticipateIn或quit状态）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("NotCompletedVolunteers")]
        [HttpGet]
        public HttpResponseMessage GetAllNotCompletedVolunteersInActivity(string id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/notcompletedvolunteers?id=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            Activity activity = myService.FindActivity(activityId);
            if (activity == null)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Activity不存在", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            List<object> source = new List<object>();
            foreach (User volunteer in myService.FindAllNotCompletedVolunteersInActivity(activity, sortByKey, isAscending, 0, 0))
            {
                var volunteerRecord = new
                {
                    VolunteerId = volunteer.Id,
                    VolunteerName = volunteer.Name,
                };
                source.Add(volunteerRecord);
            }
            List<object> result = new List<object>();
            result = myService.SortAndPaging(source, "", isAscending, pageIndex, pageSize);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得热门tag
        /// </summary>
        /// <param name="number">需要tag的数量（如果总数比number小，则返回所有）</param>
        /// <returns></returns>
        [ActionName("HotTags")]
        [HttpGet]
        public HttpResponseMessage GetHotTags(int number)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/hottags?number=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            IEnumerable<string> result = myService.ActivityTagService.FindHotTags(number);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        #region comment
        /// <summary>
        /// 用户写活动或活动总结的评论
        /// </summary>
        [ActionName("Comment")]
        [HttpPost]
        public HttpResponseMessage Comment([FromBody]CommentModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/comment") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid id = new Guid(model.id);
            BasicUser basicUser = new BasicUser(currentUser.Id, currentUser.Name);
            Guid fatherCommentId;
            if (model.isCommentOnComment == true)
            {
                try
                {
                    fatherCommentId = new Guid(model.fatherCommentId);
                }
                catch
                {
                    fatherCommentId = Guid.Empty;
                }
            }
            else
            {
                fatherCommentId = Guid.Empty;
            }
            myService.BlogService.AddComment(basicUser, id, fatherCommentId, model.content);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 获得该活动或活动总结的所有评论
        /// </summary>
        /// <param name="id">目标id</param>
        /// <param name="type">目标类型，目前为activity、summary之一</param>
        /// <param name="sortByKey"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [ActionName("Comment")]
        [HttpGet]
        public HttpResponseMessage GetComments(string id, string type, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/comment?id=&type=&sortByKey=&isAscending=&pageIndex=&pageSize=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid targetId = new Guid(id);
            IEnumerable<CommentEntity> allComments = myService.BlogService.FindAllComments(targetId, sortByKey, isAscending, pageIndex, pageSize);
            Guid activityId;
            switch (type.ToLower())
            {
                case "activity":
                    activityId = targetId;
                    break;
                case "summary":
                    var summary = myService.BlogService.FindBlogPost(targetId);
                    activityId = summary.TargetId;
                    break;
                default:
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("type参数错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Activity activity = myService.FindActivity(activityId);

            List<CommentViewModel> result = new List<CommentViewModel>();
            foreach (CommentEntity commentEntity in allComments)
            {
                //发表该评论的用户
                User user = myService.FindUser(commentEntity.User.Id);
                UserStateInActivity userState = UserStateInActivity.notSignIn;
                string avatar = null;
                foreach (Role userRole in user.UserRole)
                {
                    switch (userRole)
                    {
                        case Role.Volunteer:
                            avatar = user.UserProfiles[user.Name + "VolunteerProfile"].Avatar.AvatarPath;
                            //发表评论的用户为volunteer时，判断用户是否加入该活动
                            if (myService.CheckIfVolunteerCompleteActivity(user.Id, activity) == true)
                            {
                                userState = UserStateInActivity.complete;
                            }
                            else if (myService.CheckIfVolunteerSignInActivity(user, activityId) == true)
                            {
                                userState = UserStateInActivity.signIn;
                            }
                            else
                            {
                                userState = UserStateInActivity.notSignIn;
                            }
                            break;
                        case Role.Organizer:
                            avatar = user.UserProfiles[user.Name + "OrganizerProfile"].Avatar.AvatarPath;
                            userState = UserStateInActivity.organizer;
                            break;
                        case Role.Organization:
                            avatar = user.UserProfiles[user.Name + "OrganizationProfile"].Avatar.AvatarPath;
                            userState = UserStateInActivity.organization;
                            break;
                        case Role.Anonymous:
                            avatar = null;
                            userState = UserStateInActivity.notSignIn;
                            break;
                        default:
                            avatar = null;
                            userState = UserStateInActivity.notSignIn;
                            break;
                    }
                    //只有一个角色
                    break;
                }

                //网页显示所需信息
                CommentViewModel commentViewModel = new CommentViewModel
                {
                    Id = commentEntity.Id,
                    UserId = user.Id,
                    Avatar = avatar,
                    UserName = user.Name,
                    UserState = userState,
                    Content = myService.BlogService.GenerateDisplayContent(commentEntity),
                    Time = commentEntity.CreateTime,
                    Position = commentEntity.Position
                };
                result.Add(commentViewModel);
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 获得该活动或活动总结的所有评论的总数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("CommentCount")]
        [HttpGet]
        public HttpResponseMessage GetCommentsCount(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/commentcount?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            var result = myService.BlogService.FindAllCommentsCount(activityId);
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        #endregion comment

        #region summary
        /// <summary>
        /// 获得该活动总结的状态
        /// 0：还没有活动总结
        /// 1：草稿
        /// 2：已发布活动总结
        /// </summary>
        [ActionName("SummaryStatus")]
        [HttpGet]
        public HttpResponseMessage SummaryStatus(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/summarystatus?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            var summary = myService.BlogService.FindAllBlogPost(activityId, "", false, 0, 0).FirstOrDefault();
            ActivitySummaryStatus result;
            if (summary == null)
            {
                result = ActivitySummaryStatus.nothing;
            }
            else if (summary.IsActivated == false)
            {
                result = ActivitySummaryStatus.draft;
            }
            else
            {
                result = ActivitySummaryStatus.activated;
            }
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// 激活活动总结
        /// </summary>
        [ActionName("ActivateSummary")]
        [HttpPost]
        public HttpResponseMessage ActivateSummary([FromBody]ActivityIdModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/activatesummary") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(model.activityId);
            if (myService.BlogService.FindAllBlogPostCount(activityId) == 0)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("还没有活动总结", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.BlogService.FindAllBlogPostCount(activityId) > 1)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("活动总结过多，联系管理员解决", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var summary = myService.BlogService.FindAllBlogPost(activityId, "", false, 0, 0).FirstOrDefault();
            if (summary.IsActivated == true)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("该活动总结已经激活过", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            else
            {
                myService.BlogService.ActivateBlogPost(summary);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 获得该活动的活动总结
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("Summary")]
        [HttpGet]
        public HttpResponseMessage GetSummary(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/summary?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Guid activityId = new Guid(id);
            Activity activity =myService.FindActivity(activityId);
            User organizer=myService.FindUser(activity.OrganizerId);
            User organization=myService.FindUser(activity.OrganizationId);
            if (myService.BlogService.FindAllBlogPostCount(activityId) == 0)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("该活动还没有活动总结", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.BlogService.FindAllBlogPostCount(activityId) > 1)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("活动总结过多，联系管理员解决", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            var summary = myService.BlogService.FindAllBlogPost(activityId, "", false, 0, 0).FirstOrDefault();
            SummaryViewModel result = new SummaryViewModel
            {
                Id = summary.Id,
                ActivityId = activity.Id,
                ActivityName = activity.Name,
                OrganizerId = organizer.Id,
                OrganizerName = organizer.Name,
                OrganizerAvatar = organizer.UserProfiles[organizer.Name + "OrganizerProfile"].Avatar.AvatarPath,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                OrganizationAvatar = organization.UserProfiles[organization.Name + "OrganizationProfile"].Avatar.AvatarPath,
                Title = summary.Title,
                Content = summary.Content,
                IsActivated = summary.IsActivated,
                CreateTime = summary.CreateTime,
                ModifyTime = summary.ModifyTime
            };
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }

        /// <summary>
        /// （活动结束后）活动发起人（Organizer）写活动总结
        /// 如果已有总结，则调用该方法为修改总结
        /// </summary>
        [ActionName("Summary")]
        [HttpPost]
        public HttpResponseMessage AddSummary([FromBody]ActivitySummaryModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/summary") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(model.activityId);
            Activity activity = myService.FindActivity(activityId);
            if (activity.OrganizerId != currentUser.Id)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有活动发起人才能写活动总结", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (activity.Status != ActivityStatus.Finished)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有活动结束后才能写活动总结", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            if (myService.BlogService.FindAllBlogPostCount(activityId) > 1)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("活动总结过多，联系管理员解决", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            //已有summary，则修改
            if(myService.BlogService.FindAllBlogPostCount(activityId) == 1)
            {
                var summary = myService.BlogService.FindAllBlogPost(activityId, "", false, 0, 0).FirstOrDefault();
                myService.BlogService.ModifyBlogPost(summary, model.title, model.content);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            //还没有summary，则新建
            if (myService.BlogService.FindAllBlogPostCount(activityId) == 0)
            {
                BasicUser basicUser = new BasicUser(currentUser.Id, currentUser.Name);
                myService.BlogService.AddBlogPost(basicUser, activityId, model.title, model.content);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("服务器错误", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        }

        ///// <summary>
        ///// 修改活动总结
        ///// </summary>
        //[ActionName("Summary")]
        //[HttpPut]
        //public HttpResponseMessage ModifySummary([FromBody]ActivitySummaryModel model)
        //{
        //    if (ValidationService.AuthorizeToken(GetToken(), "put:/api/activity/summary") == false)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    User currentUser = ValidationService.FindUserWithToken(GetToken());
        //    Guid activityId = new Guid(model.activityId);
        //    Activity activity = myService.FindActivity(activityId);
        //    if (myService.BlogService.FindAllBlogPostCount(activityId) == 0)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("该活动还没有总结，无法修改", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    if (activity.OrganizerId != currentUser.Id)
        //    {
        //        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("只有活动发起人才能写活动总结", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
        //    }
        //    var summary = myService.BlogService.FindAllBlogPost(activityId, "", false, 0, 0).FirstOrDefault();
        //    myService.BlogService.ModifyBlogPost(summary, model.title, model.content);
        //    return new HttpResponseMessage(HttpStatusCode.Accepted);
        //}

        #endregion summary
        /// <summary>
        /// 对活动进行打分
        /// 只有已经完成该活动的volunteer能够打分
        /// </summary>
        [ActionName("Rate")]
        [HttpPost]
        public HttpResponseMessage RateActivity([FromBody]RateActivityModel model)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "post:/api/activity/rate") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            User currentUser = ValidationService.FindUserWithToken(GetToken());
            Guid activityId = new Guid(model.activityId);
            Activity activity = myService.FindActivity(activityId);
            try
            {
                activity.Rate(currentUser.Id, model.rate);
            }
            catch(Exception e)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message, System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// 获得活动的平均得分
        /// </summary>
        [ActionName("Rate")]
        [HttpGet]
        public HttpResponseMessage ActivityRate(string id)
        {
            if (ValidationService.AuthorizeToken(GetToken(), "get:/api/activity/rate?id=") == false)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("无访问权限", System.Text.Encoding.GetEncoding("UTF-8"), "application/text") };
            }
            Activity activity = myService.FindActivity(new Guid(id));
            double result = activity.Rating;
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, result, result.GetType());
            string json = tw.ToString();
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
    }

    public class CreateActivityModel
    {
        public string name { get; set; }
        public string organizationId { get; set; }
        public string activityAbstract { get; set; }
        public string startTime { get; set; }
        public string finishTime { get; set; }
        public double point { get; set; }
        public int leastVolunteers { get; set; }
        public int mostVolunteers { get; set; }
        public string openSignInTime { get; set; }
        public string closeSignInTime { get; set; }
        public string procedure { get; set; }
        public string coordinate { get; set; }
        public string location { get; set; }
        public string requirement { get; set; }
        public List<string> photos { get; set; }
        public List<string> videos { get; set; }
        public string activitytags { get; set; }
        public string mustGranted { get; set; }
        public string cantGranted { get; set; }
        //力量
        public double Strength { get; set; }
        //智力
        public double Intelligence { get; set; }
        //耐力
        public double Endurance { get; set; }
        //爱心
        public double Compassion { get; set; }
        //奉献
        public double Sacrifice { get; set; }
    }
    public class UpdateActivityModel
    {
        public string activityId { get; set; }
        public string name { get; set; }
        public string organizationId { get; set; }
        public string activityAbstract { get; set; }
        public string startTime { get; set; }
        public string finishTime { get; set; }
        public double point { get; set; }
        public int leastVolunteers { get; set; }
        public int mostVolunteers { get; set; }
        public string openSignInTime { get; set; }
        public string closeSignInTime { get; set; }
        public string procedure { get; set; }
        public string coordinate { get; set; }
        public string location { get; set; }
        public string requirement { get; set; }
        public List<string> photos { get; set; }
        public List<string> videos { get; set; }
        public string activitytags { get; set; }
        public string mustGranted { get; set; }
        public string cantGranted { get; set; }
        //力量
        public double Strength { get; set; }
        //智力
        public double Intelligence { get; set; }
        //耐力
        public double Endurance { get; set; }
        //爱心
        public double Compassion { get; set; }
        //奉献
        public double Sacrifice { get; set; }
    }
    public class ActivityIdModel
    {
        public string activityId { get; set; }
    }
    public class CommentModel
    {
        /// <summary>
        /// 活动或者活动总结的id，即comment的targetId
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 指示该评论是否为评论的评论
        /// </summary>
        public bool isCommentOnComment { get; set; }
        /// <summary>
        /// 当该评论为评论的评论时，被评论的评论的id
        /// </summary>
        public string fatherCommentId { get; set; }
        public string content { get; set; }
    }
    public class ActivitySummaryModel
    {
        public string activityId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
    }
    public class RateActivityModel
    {
        public string activityId { get; set; }
        public double rate { get; set; }
    }
    public class ActivityToListShow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public double Point { get; set; }
        public ActivityStatus Status { get; set; }
        public DateTime OpenSignInTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string Location { get; set; }
        public string Coordinate { get; set; }
        public string Cover { get; set; }
        public List<string> Tags { get; set; }
        public int HasSignedInVolunteerNumber { get; set; }
        public int VolunteerViewedTime { get; set; }
        public int VolunteerFavoritedTime { get; set; }
        public bool hasViewed { get; set; }
        public bool hasFavorited { get; set; }
        public bool hasSignined { get; set; }
    }
    public class VolunteerRecordToListShow
    {
        public Guid VolunteerId { get; set; }
        public string VolunteerName { get; set; }
        public Sex VolunteerSex { get; set; }
        public string VolunteerEmail { get; set; }
        public string VolunteerPhoneNumber { get; set; }
        public VolunteerStatusInActivity VolunteerStatus { get; set; }
        public SignInRecord SignedIn { get; set; }
        //check in状态
        public CheckInRecord CheckedIn { get; set; }
        //check out状态
        public CheckOutRecord CheckedOut { get; set; }
        public KickOutRecord KickedOut { get; set; }
    }
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public UserStateInActivity UserState { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
        public int Position { get; set; }
    }
    public class SummaryViewModel
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerAvatar { get; set; }
        public Guid OrganizationId { get; set; }        
        public string OrganizationName { get; set; }
        public string OrganizationAvatar { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }        
        public bool IsActivated { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }
    public enum ActivityStage
    {
        all,
        aboutToStart,
        running,
        finish
    }
    public enum UserStateInActivity
    {
        complete,
        signIn,
        notSignIn,
        organizer,
        organization
    }
    public enum ActivitySummaryStatus
    {
        /// <summary>
        /// 还没有活动总结
        /// </summary>
        nothing,
        /// <summary>
        /// 草稿
        /// </summary>
        draft,
        /// <summary>
        /// 已发布活动总结
        /// </summary>
        activated
    }
}