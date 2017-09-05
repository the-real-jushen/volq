using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Repository;
using Jtext103.Repository.Interface;
using Jtext103.MongoDBProvider;
using Jtext103.Volunteer.VolunteerMessage;
using Jtext103.Volunteer.Tag;
using Jtext103.Volunteer.Badge;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.Volunteer.ActionValidation;
using System.Security.Cryptography;
using Jtext103.BlogSystem;
using System.IO;

namespace Jtext103.Volunteer.Service
{
    public class VolunteerService : IVolunteerService
    {
        public static VolunteerService Instance;
        public FriendServiceInVolunteerService FriendServiceInVolunteerService;
        public MessageService MessageService;
        public MessageService FeedService;
        public TagService ActivityTagService;
        public TagService AffiliationService;
        public ActionValidationService ActionValidationService;
        public BlogService BlogService;
        private IRepository<Entity> entityRepository;

        private Thread checkEveryHourthread;

        public VolunteerService(IRepository<Entity> entityRepository)
        {
            this.entityRepository = entityRepository;

            Activity.RegisterMe(this);
            User.RegisterMe(this);
            //Instance = this;
        }

        public void InitVolunteerService(MessageService messageService, MessageService feedService, TagService activityTagService, TagService affiliationService, ActionValidationService actionValidationService, BlogService blogService)
        {
            Instance = this;

            //启动每一个小时启动一次的线程
            checkEveryHourthread = new Thread(this.checkEveryHour);
            checkEveryHourthread.Start();

            //new FriendServiceInVolunteerService
            FriendServiceInVolunteerService = new FriendServiceInVolunteerService(entityRepository, this);

            //初始化MessageService和FeedService
            this.MessageService = messageService;
            this.FeedService = feedService;
            //初始化ActivityTagService和AffiliationService
            this.ActivityTagService = activityTagService;
            this.AffiliationService = affiliationService;
            //初始化ActionValidationService
            this.ActionValidationService = actionValidationService;
            //初始化CommentService
            this.BlogService = blogService;

            //首先获得所有非finish、非abort、非draft状态下的所有activity
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$nin", new List<ActivityStatus>() { ActivityStatus.Finished, ActivityStatus.Abort });
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("isActive", true);
            queryDict.Add("statusInDB", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<Activity> activities = switchToActivityList(source);
            foreach (Activity activity in activities)
            {
                //根据activity的状态，新建最多4个定时器检查activity的状态
                //先重置db中的statusInDB, 即get status
                //再定时启动线程, 每次都get status，重置db中的statusInDB
                TimeToCheckStatus(activity);
            }
            
        }

        #region IVolunteerService

        #region register
        /// <summary>
        /// register the list to mongodb database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyList">the list</param>
        public void RegisterMap<T>(IEnumerable<string> propertyList)
        {
            entityRepository.RegisterMap<T>(propertyList);
        }
        /// <summary>
        /// register to mongodb database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterMap<T>()
        {
            entityRepository.RegisterMap<T>();
        }
        #endregion register

        #region insert&save&update
        public void InsertOne(Entity entity)
        {
            entityRepository.InsertOne(entity);
        }

        public void SaveOne(Entity entity)
        {
            entityRepository.SaveOne(entity);
        }

        public void UpdateOne(Guid id, Dictionary<string, object> updateObject)
        {
            Dictionary<string, object> queryObject = new Dictionary<string, object>();
            queryObject.Add("_id", id);
            entityRepository.Update(queryObject, updateObject);
        }
        #endregion insert&save&update

        #region find
        public Entity FindOneById(Guid id)
        {
            return entityRepository.FindOneById(id);
        }
        public User FindUser(string email)
        {
            User user;
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("Email", email.ToLowerInvariant().Trim());
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            user = (User)entityRepository.Find(queryObject).FirstOrDefault();
            return user;
        }
        public User FindUser(string email, string password)
        {
            User user;
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("Email", email.ToLowerInvariant().Trim());
            queryDict.Add("Password", MD5Encrypt(password));
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            user = (User)entityRepository.Find(queryObject).FirstOrDefault();
            return user;
        }
        public User FindUser(Guid userId)
        {
            return (User)entityRepository.FindOneById(userId);
        }
        public Activity FindActivity(Guid activityId)
        {
            return (Activity)entityRepository.FindOneById(activityId);
        }
        public IEnumerable<Activity> FindAllActivities(string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "Activity");
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        public IEnumerable<Activity> FindAllNotDraftActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("isActive", true);
            //过滤user
            if (user != null)
            {
                //为volunteer时，该volunteer参与的活动
                if (user.UserRole.Contains(Role.Volunteer))
                {
                    //该volunteer参与的活动
                    List<Guid> signedInActivityIds = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).SignedInActivityIds.ToList();
                    Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
                    subQueryDict1.Add("$in", signedInActivityIds);
                    queryDict.Add("_id", subQueryDict1);
                }
                else if (user.UserRole.Contains(Role.Organization))
                {
                    queryDict.Add("OrganizationId", user.Id);
                }
                else if (user.UserRole.Contains(Role.Organizer))
                {
                    //List<Guid> organizationIds = ((OrganizerProfile)user.UserProfiles[user.Name + "OrganizerProfile"]).MyOrganizations.Keys.ToList();
                    //Dictionary<string, object> subQueryDict4 = new Dictionary<string, object>();
                    //subQueryDict4.Add("$in", organizationIds);
                    //queryDict.Add("OrganizationId", subQueryDict4);
                    queryDict.Add("OrganizerId", user.Id);
                }
            }
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //找到所有organization或organizer名下的draft状态的活动
        //当为organizer时，返回该organizer创建的所有draft状态的活动        
        public List<Activity> FindDraftActivitiesByOrganizerOrOrganization(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            foreach (Role role in user.UserRole)
            {
                switch (role)
                {
                    case Role.Organizer:
                        //以下注释部分已经过时
                        ////当为organizer时，返回所有该organizer所属的organization名下的draft状态的活动
                        //List<Guid> belongToOrganization = new List<Guid>();
                        //foreach (User organization in FindAllJoinedOrganizationByOrganizer(user, "", false, 0, 0))
                        //{
                        //    belongToOrganization.Add(organization.Id);
                        //}
                        //Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
                        //subQueryDict.Add("$in", belongToOrganization);
                        //queryDict.Add("OrganizationId", subQueryDict);
                        queryDict.Add("OrganizerId", user.Id);
                        break;
                    case Role.Organization:
                        queryDict.Add("OrganizationId", user.Id);
                        break;
                    //如果用户角色不是organizer或organization，则返回空列表
                    default:
                        return new List<Activity>();
                }
                //目前user只有一个角色
                break;
            }
            queryDict.Add("isActive", false);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该volunteer 已经sign in的activity,且该volunteer未sign out
        public List<Activity> FindActivitesVolunteerSignedIn(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> signedInActivityIds = new List<Guid>();
            foreach (Guid id in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds)
            {
                signedInActivityIds.Add(id);
            }
            subQueryDict.Add("$in", signedInActivityIds);
            queryDict.Add("_id", subQueryDict);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该organization名下的activity
        public List<Activity> FindActivitesByOrganizationId(Guid organizationId, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("OrganizationId", organizationId);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该organizer创建的且已经激活的activity
        public List<Activity> FindActivatedActivitesByOrganizerId(Guid organizerId, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("OrganizerId", organizerId);
            queryDict.Add("isActive", true);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该user(volunteer或者organization)即将开始的activity,如果user为null,则返回所有(处于active、maxVolunteer、ready、signIn、RunningSignInAndCheckIn状态下的)
        public IEnumerable<Activity> FindAllAboutToStartActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.Active, ActivityStatus.MaxVolunteer, ActivityStatus.Ready, ActivityStatus.SignIn, ActivityStatus.RunningSignInAndCheckIn });
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("statusInDB", subQueryDict);
            //过滤user
            if (user != null)
            {
                //为volunteer时，该volunteer参与且还未完成的活动
                if (user.UserRole.Contains(Role.Volunteer))
                {
                    //该volunteer参与的活动
                    List<Guid> signedInActivityIds = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).SignedInActivityIds.ToList();
                    Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
                    subQueryDict1.Add("$in", signedInActivityIds);
                    queryDict.Add("_id", subQueryDict1);
                    //该volunteer未完成的活动
                    Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryDict3 = new Dictionary<string, object>();
                    subQueryDict3.Add("VolunteerId", user.Id);
                    subQueryDict3.Add("CheckedOut.IsCheckedOut", false);
                    subQueryDict2.Add("$elemMatch", subQueryDict3);
                    queryDict.Add("VolunteerStatus", subQueryDict2);
                }
                else if (user.UserRole.Contains(Role.Organization))
                {
                    queryDict.Add("OrganizationId", user.Id);
                }
                else if (user.UserRole.Contains(Role.Organizer))
                {
                    //List<Guid> organizationIds = ((OrganizerProfile)user.UserProfiles[user.Name + "OrganizerProfile"]).MyOrganizations.Keys.ToList();
                    //Dictionary<string, object> subQueryDict4 = new Dictionary<string, object>();
                    //subQueryDict4.Add("$in", organizationIds);
                    //queryDict.Add("OrganizationId", subQueryDict4);
                    queryDict.Add("OrganizerId", user.Id);
                }
            }
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该user(volunteer或者organization)即将开始的activity,如果user为null,则返回所有(处于active、maxVolunteer、ready、signIn、RunningSignInAndCheckIn状态下的, 且距当前24小时以内的活动)
        private IEnumerable<Activity> FindAllAboutToStartIn24hActivities()
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.Active, ActivityStatus.MaxVolunteer, ActivityStatus.Ready, ActivityStatus.SignIn, ActivityStatus.RunningSignInAndCheckIn });
            subQueryDict1.Add("$lt", DateTime.Now + new TimeSpan(24, 0, 0));
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("statusInDB", subQueryDict);
            queryDict.Add("StartTime", subQueryDict1);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该user正在进行的activity,如果user为null,则返回所有(处于RunningCheckIn、RunningSignInAndCheckIn状态下的)
        public IEnumerable<Activity> FindAllRunningActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.RunningCheckIn, ActivityStatus.RunningSignInAndCheckIn });
            queryDict.Add("EntityType", "Activity");
            queryDict.Add("statusInDB", subQueryDict);
            //过滤user
            if (user != null)
            {
                //为volunteer时，该volunteer参与且还未完成的活动
                if (user.UserRole.Contains(Role.Volunteer))
                {
                    //该volunteer参与的活动
                    List<Guid> signedInActivityIds = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).SignedInActivityIds.ToList();
                    Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
                    subQueryDict1.Add("$in", signedInActivityIds);
                    queryDict.Add("_id", subQueryDict1);
                    //该volunteer未完成的活动
                    Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryDict3 = new Dictionary<string, object>();
                    subQueryDict3.Add("VolunteerId", user.Id);
                    subQueryDict3.Add("CheckedOut.IsCheckedOut", false);
                    subQueryDict2.Add("$elemMatch", subQueryDict3);
                    queryDict.Add("VolunteerStatus", subQueryDict2);
                }
                else if (user.UserRole.Contains(Role.Organization))
                {
                    queryDict.Add("OrganizationId", user.Id);
                }
                else if (user.UserRole.Contains(Role.Organizer))
                {
                    //List<Guid> organizationIds = ((OrganizerProfile)user.UserProfiles[user.Name + "OrganizerProfile"]).MyOrganizations.Keys.ToList();
                    //Dictionary<string, object> subQueryDict4 = new Dictionary<string, object>();
                    //subQueryDict4.Add("$in", organizationIds);
                    //queryDict.Add("OrganizationId", subQueryDict4);
                    queryDict.Add("OrganizerId", user.Id);
                }
            }
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取所有该user(volunteer或者organization)已经完成的activity,如果user为null,则返回所有(处于Finished状态下的)
        //当user为volunteer时，返回该volunteer已经check out的活动（可能会包括不处于Finish状态的活动）
        //当user不为volunteer时，返回处于Finished状态的活动
        public IEnumerable<Activity> FindAllFinishedActivities(User user, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "Activity");            
            //过滤user
            if (user != null)
            {
                //为volunteer时，该volunteer参与且check out的活动
                if (user.UserRole.Contains(Role.Volunteer))
                {
                    //该volunteer参与的活动
                    List<Guid> signedInActivityIds = ((VolunteerProfile)user.UserProfiles[user.Name + "VolunteerProfile"]).SignedInActivityIds.ToList();
                    Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
                    subQueryDict1.Add("$in", signedInActivityIds);
                    queryDict.Add("_id", subQueryDict1);
                    //该volunteer已经check out的活动
                    Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryDict3 = new Dictionary<string, object>();
                    subQueryDict3.Add("VolunteerId", user.Id);
                    subQueryDict3.Add("CheckedOut.IsCheckedOut", true);
                    subQueryDict2.Add("$elemMatch", subQueryDict3);
                    queryDict.Add("VolunteerStatus", subQueryDict2);
                }
                else if (user.UserRole.Contains(Role.Organization))
                {
                    subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.Finished });
                    queryDict.Add("statusInDB", subQueryDict);
                    queryDict.Add("OrganizationId", user.Id);
                }
                else if (user.UserRole.Contains(Role.Organizer))
                {
                    subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.Finished });
                    queryDict.Add("statusInDB", subQueryDict);
                    //List<Guid> organizationIds = ((OrganizerProfile)user.UserProfiles[user.Name + "OrganizerProfile"]).MyOrganizations.Keys.ToList();
                    //Dictionary<string, object> subQueryDict4 = new Dictionary<string, object>();
                    //subQueryDict4.Add("$in", organizationIds);
                    //queryDict.Add("OrganizationId", subQueryDict4);
                    queryDict.Add("OrganizerId", user.Id);
                }
            }
            else
            {
                subQueryDict.Add("$in", new List<ActivityStatus>() { ActivityStatus.Finished });
                queryDict.Add("statusInDB", subQueryDict);
            }
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        public IEnumerable<User> FindAllUsers(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "User");
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        public IEnumerable<User> FindAllOrganizations(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            List<Role> userRole = new List<Role>();
            userRole.Add(Role.Organization);
            queryDict.Add("EntityType", "User");
            queryDict.Add("UserRole", userRole);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> allOrganizations = switchToUserList(source);
            return allOrganizations;
        }
        public IEnumerable<User> FindAllOrganizers(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            List<Role> userRole = new List<Role>();
            userRole.Add(Role.Organizer);
            queryDict.Add("EntityType", "User");
            queryDict.Add("UserRole", userRole);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> allOrganizers = switchToUserList(source);
            return allOrganizers;
        }
        public IEnumerable<User> FindAllVolunteers(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            List<Role> userRole = new List<Role>();
            userRole.Add(Role.Volunteer);
            queryDict.Add("EntityType", "User");
            queryDict.Add("UserRole", userRole);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> allVolunteers = switchToUserList(source);
            return allVolunteers;
        }
        //获取所有申请当前organization的organizer
        public IEnumerable<User> FindAllAppliedOrganizerByOrganization(User organization, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> applyOrganizerIds = new List<Guid>();
            foreach (var information in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation)
            {
                if (information.hasHandled == false)
                {
                    applyOrganizerIds.Add(information.OrganizerId);
                }
            }
            subQueryDict.Add("$in", applyOrganizerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取当前organization所有organizer成员
        public IEnumerable<User> FindAllOrganizerByOrganization(User organization, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> organizerIds = new List<Guid>();
            foreach (Guid organizerId in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds)
            {
                organizerIds.Add(organizerId);
            }
            subQueryDict.Add("$in", organizerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取当前organizer已申请但还未接受或拒绝的organization
        public IEnumerable<User> FindAllAppliedOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> appliedOrganizationIds = new List<Guid>();
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                if (information.Status == ApplyOrganizationStatus.Applying)
                {
                    appliedOrganizationIds.Add(information.ApplyOrganizationId);
                }
            }
            subQueryDict.Add("$in", appliedOrganizationIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取当前organizer已加入的organization
        public IEnumerable<User> FindAllJoinedOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> organizationIds = new List<Guid>();
            foreach (Guid organizationId in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys)
            {
                organizationIds.Add(organizationId);
            }
            subQueryDict.Add("$in", organizationIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取当前Organizer可以加入的Organizations,即未加入也未申请的Organizations
        public IEnumerable<User> FindAllToJoinOrganizationByOrganizer(User organizer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            //所有organization
            List<User> allOrganization = (List<User>)FindAllOrganizations("", false, 0, 0);
            List<Guid> allOrganizationIds = new List<Guid>();
            foreach (User organization in allOrganization)
            {
                allOrganizationIds.Add(organization.Id);
            }
            //已申请的organization
            List<Guid> appliedOrganizationIds = new List<Guid>();
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                if (information.Status == ApplyOrganizationStatus.Applying)
                {
                    appliedOrganizationIds.Add(information.ApplyOrganizationId);
                }
            }
            //已加入的organization
            List<Guid> joinedOrganizationIds = new List<Guid>();
            foreach (Guid organizationId in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys)
            {
                joinedOrganizationIds.Add(organizationId);
            }
            //排除已加入和已申请的organization
            foreach (Guid appliedOrganizationId in appliedOrganizationIds)
            {
                allOrganizationIds.Remove(appliedOrganizationId);
            }
            foreach (Guid joinedOrganizationId in joinedOrganizationIds)
            {
                allOrganizationIds.Remove(joinedOrganizationId);
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", allOrganizationIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取当前volunteer所有已经signed in的activity,且该volunteer处于signedIn状态
        public IEnumerable<Activity> FindAllVolunteerSignedInActivities(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> signedInActivityIds = new List<Guid>();
            foreach (Guid id in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds)
            {
                signedInActivityIds.Add(id);
            }
            subQueryDict.Add("$in", signedInActivityIds);
            queryDict.Add("_id", subQueryDict);
            //VolunteerStatus == VolunteerStatusInActivity.signedIn 处于signedIn状态
            queryDict.Add("VolunteerStatus.SignedIn.IsSignedIn", true);
            queryDict.Add("VolunteerStatus.CheckedIn.IsCheckedIn", false);
            queryDict.Add("VolunteerStatus.CheckedOut.IsCheckedOut", false);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        /*
        //获取当前volunteer所有已经signed in的activity,且该volunteer未sign out
        public IEnumerable<Activity> FindAllVolunteerNotSignOutActivites(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            List<Guid> signedInActivityIds = new List<Guid>();
            foreach (Guid id in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds)
            {
                signedInActivityIds.Add(id);
            }
            subQueryDict.Add("$in", signedInActivityIds);
            queryDict.Add("_id", subQueryDict);
            //VolunteerStatus == VolunteerStatusInActivity.unsignedIn 处于unsignedIn状态
            queryDict.Add("VolunteerStatus.SignedIn.IsSignedIn", true);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        */
        //获取当前volunteer所有已经完成的activity
        public IEnumerable<Activity> FindAllVolunteerCompletedActivities(User volunteer, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("VolunteerId", volunteer.Id);
            subQueryDict2.Add("KickedOut.IsKickedOut", false);
            subQueryDict2.Add("SignedIn.IsSignedIn", true);
            subQueryDict2.Add("CheckedIn.IsCheckedIn", true);
            subQueryDict2.Add("CheckedOut.IsCheckedOut",true);
            subQueryDict2.Add("CheckedOut.Status", CheckOutStatus.complete);
            subQueryDict1.Add("$elemMatch", subQueryDict2);
            queryDict.Add("VolunteerStatus", subQueryDict1);
            queryDict.Add("EntityType", "Activity");
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> result = switchToActivityList(source);
            return result;
            ////在查找数据库时进行排序，之后则不需排序
            //List<Activity> allActivities = FindActivitesVolunteerSignedIn(volunteer, "", sortByKey, isAscending, 0, 0);
            //if (allActivities.Any())
            //{
            //    List<Activity> source = new List<Activity>();
            //    foreach (Activity activity in allActivities)
            //    {
            //        foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
            //        {
            //            if (record.VolunteerId == volunteer.Id)
            //            {
            //                if (record.VolunteerStatus == VolunteerStatusInActivity.complete)
            //                {
            //                    source.Add(activity);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    List<Activity> result = SortAndPaging(source, "", isAscending, pageIndex, pageSize);//只分页不排序
            //    return result;
            //}
            //else return new List<Activity>();
        }
        //获取所有已经参加给定activity的volunteer
        public IEnumerable<User> FindAllVolunteerInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> volunteerIds = new List<Guid>();
            foreach (VolunteerParticipateInActivityRecord volunteerStatus in activity.VolunteerStatus)
            {
                if (volunteerStatus.VolunteerStatus != VolunteerStatusInActivity.unsignedIn && volunteerStatus.VolunteerStatus != VolunteerStatusInActivity.kickedOut && volunteerStatus.VolunteerStatus != VolunteerStatusInActivity.error)
                {
                    volunteerIds.Add(volunteerStatus.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", volunteerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获得所有能够在该activity中check in的volunteer(目前处于signedIn状态)
        public IEnumerable<User> FindAllToCheckInVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> volunteerIds = new List<Guid>();
            foreach (VolunteerParticipateInActivityRecord volunteerStatus in activity.VolunteerStatus)
            {
                if (volunteerStatus.VolunteerStatus == VolunteerStatusInActivity.signedIn)
                {
                    volunteerIds.Add(volunteerStatus.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", volunteerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获得所有能够在该activity中check out的volunteer(目前处于checkedIn状态)
        public IEnumerable<User> FindAllToCheckOutVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> volunteerIds = new List<Guid>();
            foreach (VolunteerParticipateInActivityRecord volunteerStatus in activity.VolunteerStatus)
            {
                if (volunteerStatus.VolunteerStatus == VolunteerStatusInActivity.checkedIn)
                {
                    volunteerIds.Add(volunteerStatus.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", volunteerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获得所有完成该activity的volunteer(处于complete状态)
        public IEnumerable<User> FindAllCompletedVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> volunteerIds = new List<Guid>();
            foreach (VolunteerParticipateInActivityRecord volunteerStatus in activity.VolunteerStatus)
            {
                if (volunteerStatus.VolunteerStatus == VolunteerStatusInActivity.complete)
                {
                    volunteerIds.Add(volunteerStatus.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", volunteerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获得所有未完成该activity的volunteer(处于notParticipateIn或quit状态)
        public IEnumerable<User> FindAllNotCompletedVolunteersInActivity(Activity activity, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> volunteerIds = new List<Guid>();
            foreach (VolunteerParticipateInActivityRecord volunteerStatus in activity.VolunteerStatus)
            {
                if (volunteerStatus.VolunteerStatus == VolunteerStatusInActivity.notParticipateIn || volunteerStatus.VolunteerStatus == VolunteerStatusInActivity.quit)
                {
                    volunteerIds.Add(volunteerStatus.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", volunteerIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获得所有收藏该activity的所有volunteer
        public IEnumerable<User> FindAllVolunteerWhoFavoriteTheActivity(Guid activityId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("ActivityId", activityId);
            subQueryDict2.Add("IsViewOrFavorite", true);
            subQueryDict1.Add("$elemMatch", subQueryDict2);
            queryDict.Add("_userProfiles.VolunteerFavoriteActivitiesRecords", subQueryDict1);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> users = switchToUserList(source);
            return users;
        }
        //获取所有给定用户收藏的activity
        public IEnumerable<Activity> FindAllActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> activityIds = new List<Guid>();
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.IsViewOrFavorite == true)
                {
                    activityIds.Add(record.ActivityId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", activityIds);
            queryDict.Add("_id", subQueryDict);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //用户收藏的处于即将开始状态的活动(处于active、maxVolunteer、ready、signIn、RunningSignInAndCheckIn状态下的)
        public IEnumerable<Activity> FindAboutToStartActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> activityIds = new List<Guid>();
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.IsViewOrFavorite == true)
                {
                    activityIds.Add(record.ActivityId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$in", activityIds);
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("$in", new List<ActivityStatus>() { ActivityStatus.Active, ActivityStatus.MaxVolunteer, ActivityStatus.Ready, ActivityStatus.SignIn, ActivityStatus.RunningSignInAndCheckIn });
            queryDict.Add("_id", subQueryDict1);
            queryDict.Add("statusInDB", subQueryDict2);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //用户收藏的处于正在进行状态的活动(处于RunningCheckIn、RunningSignInAndCheckIn状态下的)
        public IEnumerable<Activity> FindRunningActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> activityIds = new List<Guid>();
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.IsViewOrFavorite == true)
                {
                    activityIds.Add(record.ActivityId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$in", activityIds);
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("$in", new List<ActivityStatus>() { ActivityStatus.RunningCheckIn, ActivityStatus.RunningSignInAndCheckIn });
            queryDict.Add("_id", subQueryDict1);
            queryDict.Add("statusInDB", subQueryDict2);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //用户收藏的已经结束的活动(处于Finished状态下的)
        public IEnumerable<Activity> FindFinishedActivitiesWhichVolunteerFavorite(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> activityIds = new List<Guid>();
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.IsViewOrFavorite == true)
                {
                    activityIds.Add(record.ActivityId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$in", activityIds);
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("$in", new List<ActivityStatus>() { ActivityStatus.Finished });
            queryDict.Add("_id", subQueryDict1);
            queryDict.Add("statusInDB", subQueryDict2);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //获取用户查看过的activity（历史记录）
        public IEnumerable<Activity> FindAllActivitiesWhichVolunteerViewed(User volunteer, string filterSource, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> activityIds = new List<Guid>();
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerViewActivitiesRecords)
            {
                if (record.IsViewOrFavorite == true)
                {
                    activityIds.Add(record.ActivityId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", activityIds);
            queryDict.Add("_id", subQueryDict);
            //过滤
            QueryObject<Entity> queryObject = generateActivityFilter(filterSource);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<Activity> activities = switchToActivityList(source);
            return activities;
        }
        //找到一条信息的发送人（如果是系统等没存在数据库中的，则返回message.MessageFrom字符串）
        public object FindMessageSender(Message message)
        {
            try
            {
                Guid senderId = new Guid(message.MessageFrom);
                return entityRepository.FindOneById(senderId);
            }
            catch
            {
                return message.MessageFrom;
            }
        }
        internal List<User> switchToUserList(IEnumerable<Entity> source)
        {
            List<User> result = new List<User>();
            foreach (Entity user in source)
            {
                result.Add((User)user);
            }
            return result;
        }
        internal List<Activity> switchToActivityList(IEnumerable<Entity> source)
        {
            List<Activity> result = new List<Activity>();
            foreach (Entity activity in source)
            {
                result.Add((Activity)activity);
            }
            return result;
        }
        #endregion find

        #region other
        /// <summary>
        /// 为organization添加一个organizer成员
        /// </summary>
        /// <param name="organizer">要添加的organizer</param>
        /// <param name="organization">organization</param>
        /// <returns></returns>
        public bool AddOrganizerToOrganization(User organizer, User organization)
        {
            //（organization profile中显示）organizer已在organization中
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(organizer.Id))
            {
                //throw new Exception("organizer " + organizer.Name + " 已在该organization " + organization.Name + " 中");
                return false;
            }
            //（organizer profile中显示）organizer已在organization中
            if (((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys.Contains(organization.Id))
            {
                //throw new Exception("organizer " + organizer.Name + " 已在该organization " + organization.Name + " 中");
                return false;
            }
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Add(organizer.Id);
            ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Add(organization.Id, DateTime.Now);
            SaveOne(organizer);
            SaveOne(organization);
            return true;
        }

        /// <summary>
        /// organization踢掉一个organizer成员
        /// </summary>
        /// <param name="organizer">organizer</param>
        /// <param name="organization">organization</param>
        /// <param name="comment">备注</param>
        /// <returns></returns>
        public bool OrganizationKickOrganizerOut(User organizer, User organization, string comment)
        {
            bool removeFromOrganization = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Remove(organizer.Id);
            bool removeFromOrganizer = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Remove(organization.Id);
            bool findInformation = false;
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                //将information中接受申请的记录修改为kickout
                if ((information.ApplyOrganizationId == organization.Id) && (information.Status == ApplyOrganizationStatus.Accept))
                {
                    information.Status = ApplyOrganizationStatus.KickOut;
                    information.ActionTime = DateTime.Now;
                    information.Comment = comment;
                    findInformation = true;
                    break;
                }
            }
            if (removeFromOrganization & removeFromOrganizer & findInformation == false)
            {
                return false;
            }
            SaveOne(organizer);
            SaveOne(organization);
            //产生OrganizationKickOrganizerOutEvent事件
            EventService.Publish("OrganizationKickOrganizerOutEvent", organizer.Id.ToString() + "," + organization.Id.ToString() + "," + comment, organization.Id);
            return true;
        }

        /// <summary>
        /// organizer主动离开一个organization
        /// </summary>
        /// <param name="organizer"></param>
        /// <param name="organization"></param>
        /// <returns></returns>
        public bool OrganizerQuitOrganization(User organizer, User organization)
        {
            bool removeFromOrganization = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Remove(organizer.Id);
            bool removeFromOrganizer = ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Remove(organization.Id);
            bool findInformation = false;
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                //将information中接受申请的记录修改为quit
                if ((information.ApplyOrganizationId == organization.Id) && (information.Status == ApplyOrganizationStatus.Accept))
                {
                    information.Status = ApplyOrganizationStatus.Quit;
                    information.ActionTime = DateTime.Now;
                    information.Comment = "主动退出";
                    findInformation = true;
                    break;
                }
            }
            if (removeFromOrganization & removeFromOrganizer & findInformation == false)
            {
                return false;
            }
            SaveOne(organizer);
            SaveOne(organization);
            return true;
        }

        /// <summary>
        /// organizer申请加入organization
        /// </summary>
        /// <param name="organizer"></param>
        /// <param name="organization"></param>
        /// <param name="comment">organizer申请加入的理由</param>
        /// <returns></returns>
        public bool OrganizerApplyToJoinOrganization(User organizer, User organization, string comment)
        {
            //（organization profile中显示）organizer已在organization中
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(organizer.Id))
            {
                return false;
            }
            //（organizer profile中显示）organizer已在organization中
            if (((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys.Contains(organization.Id))
            {
                return false;
            }
            //（organization profile中显示）organizer已在organization申请列表中
            foreach (var information in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation)
            {
                if ((information.OrganizerId == organizer.Id) && (information.hasHandled == false))
                {
                    return false;
                }
            }
            //（organizer profile中显示）organizer已在organization申请列表中
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                if ((information.ApplyOrganizationId == organization.Id) && (information.Status == ApplyOrganizationStatus.Applying))
                {
                    return false;
                }
            }
            //分别加入organization profile和organizer profile的申请列表中
            ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation.Add(new ApplyOrganizerInformationByOrganization(organizer.Id, organizer.Name, comment));
            ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation.Add(new ApplyOrganizationInformationByOrganizer(organization.Id, organization.Name));
            SaveOne(organizer);
            SaveOne(organization);
            return true;
        }

        /// <summary>
        /// organization接受organizer的申请
        /// </summary>
        /// <param name="organizer">organizer</param>
        /// <param name="organization">organization</param>
        /// <returns></returns>
        public bool OrganizationAcceptOrganizerApplication(User organizer, User organization, string comment)
        {
            //（organization profile中显示）organizer已在organization中
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(organizer.Id))
            {
                return false;
            }
            //（organizer profile中显示）organizer已在organization中
            if (((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys.Contains(organization.Id))
            {
                return false;
            }
            //organization profile将information中hasHandled改为true
            foreach (var information in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation)
            {
                if ((information.OrganizerId == organizer.Id) && (information.hasHandled == false))
                {
                    information.hasHandled = true;
                }
            }
            //organizer profile将information中申请的记录修改为accept
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                if ((information.ApplyOrganizationId == organization.Id) && (information.Status == ApplyOrganizationStatus.Applying))
                {
                    information.Status = ApplyOrganizationStatus.Accept;
                    information.ActionTime = DateTime.Now;
                    information.Comment = comment;
                    break;
                }
            }
            //产生OrganizationAcceptOrganizerApplicationEvent事件
            EventService.Publish("OrganizationAcceptOrganizerApplicationEvent", organizer.Id.ToString() + "," + organization.Id.ToString() + "," + comment, organization.Id);
            //为organization添加organizer成员
            return AddOrganizerToOrganization(organizer, organization);
        }

        /// <summary>
        /// organization拒绝organizer的申请
        /// </summary>
        /// <param name="organizer">organizer</param>
        /// <param name="organization">organization</param>
        /// <returns></returns>
        public bool OrganizationRefuseOrganizerApplication(User organizer, User organization, string comment)
        {
            //（organization profile中显示）organizer已在organization中
            if (((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).OrganizerIds.Contains(organizer.Id))
            {
                return false;
            }
            //（organizer profile中显示）organizer已在organization中
            if (((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).MyOrganizations.Keys.Contains(organization.Id))
            {
                return false;
            }
            //organization profile将information中hasHandled改为true
            foreach (var information in ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).ApplyOrganizerInformation)
            {
                if ((information.OrganizerId == organizer.Id) && (information.hasHandled == false))
                {
                    information.hasHandled = true;
                }
            }
            //organizer profile将information中申请的记录修改为refuse
            foreach (ApplyOrganizationInformationByOrganizer information in ((OrganizerProfile)organizer.UserProfiles[organizer.Name + "OrganizerProfile"]).ApplyOrganizationInformation)
            {
                if ((information.ApplyOrganizationId == organization.Id) && (information.Status == ApplyOrganizationStatus.Applying))
                {
                    information.Status = ApplyOrganizationStatus.Refuse;
                    information.ActionTime = DateTime.Now;
                    information.Comment = comment;
                    break;
                }
            }
            SaveOne(organizer);
            SaveOne(organization);
            //产生OrganizationRefuseOrganizerApplicationEvent事件
            EventService.Publish("OrganizationRefuseOrganizerApplicationEvent", organizer.Id.ToString() + "," + organization.Id.ToString() + "," + comment, organization.Id);
            return true;
        }
        /*
        /// <summary>
        /// volunteer加入一个Activity
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <param name="activity">activity</param>
        /// <returns></returns>
        public bool VolunteerSignInActivity(User volunteer, Activity activity)
        {
            if (activity.Status == ActivityStatus.SignIn)
            {
                //如果该activity已经在volunteerProfile.SigninedActivityIds中，则说明用户曾经sign in过后来取消，则不需要修改VolunteerProfile
                //如果该activity不在volunteerProfile.SigninedActivityIds中，则说明用户首次sign in，须在VolunteerProfile中添加activity.Id
                if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Contains(activity.Id))
                {
                    ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Add(activity.Id);
                }

                //将该volunteer 加入 activity.volunteerStatus
                //如果该volunteer已经sign in后取消，则只需修改状态
                foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
                {
                    if (record.VolunteerId == volunteer.Id)
                    {
                        if (record.VolunteerStatus == VolunteerStatusInActivity.unsignedIn)
                        {
                            record.SignInTime = DateTime.Now;
                            record.SignedIn.IsSignedIn = true;
                            volunteer.Save();
                            activity.Save();
                            return true;
                        }
                        else return false;
                    }
                }
                //如果该volunteer从未sign in，则需新建volunteerRecord
                VolunteerParticipateInActivityRecord volunteerRecord = new VolunteerParticipateInActivityRecord(volunteer.Id, DateTime.Now);
                activity.VolunteerStatus.Add(volunteerRecord);
                SaveOne(volunteer);
                SaveOne(activity);
                return true;
            }
            else return false;
        }
        */
        /*
        /// <summary>
        /// volunteer sign in 后取消 sign in Activity
        /// </summary>
        /// <param name="volunteer">volunteer</param>
        /// <param name="activity">activity</param>
        /// <returns></returns>
        public bool VolunteerSignOutActivity(User volunteer, Activity activity)
        {
            //判断volunteerProfile.SigninedActivityIds中是否有activity id
            if (!((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).SignedInActivityIds.Contains(activity.Id))
            {
                //该activity不在volunteerProfile.SigninedActivityIds中
                return false;
            }
            bool IsRecortInActivity = false;//标志该volunteer是否在activity.volunteerStatus中
            //改变activity.volunteerStatus中状态
            foreach (VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
            {
                if (record.VolunteerId == volunteer.Id)
                {
                    record.SignedIn.IsSignedIn = false;
                    IsRecortInActivity = true;
                    break;
                }
            }
            if (IsRecortInActivity == false)
            {
                //该volunteer不在activity.volunteerStatus中
                return false;
            }
            SaveOne(volunteer);
            SaveOne(activity);
            return true;
        }
        */
        /// <summary>
        /// 志愿者查看一条activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        public void VolunteerViewActivity(User volunteer, Activity activity)
        {
            //指示是否已有记录
            bool hasRecord = false;
            //如果已有记录则更新
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerViewActivitiesRecords)
            {
                if (record.ActivityId == activity.Id)
                {
                    record.IsViewOrFavorite = true;
                    record.WhenViewOrFavorite = DateTime.Now;
                    hasRecord = true;
                    break;
                }
            }
            //否则新加记录
            if (hasRecord == false)
            {
                VolunteerViewOrFavoriteRecord volunteerViewActivities = new VolunteerViewOrFavoriteRecord(activity.Id);
                volunteerViewActivities.IsViewOrFavorite = true;
                volunteerViewActivities.WhenViewOrFavorite = DateTime.Now;
                ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerViewActivitiesRecords.Add(volunteerViewActivities);
                activity.VolunteerViewedTime++;
                SaveOne(activity);
            }
            SaveOne(volunteer);
        }

        /// <summary>
        /// 清除志愿者查看activity的历史记录
        /// </summary>
        /// <param name="volunteer"></param>
        public void ClearVolunteerViewActivityRecord(User volunteer)
        {
            foreach (var volunteerViewActivityRecord in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerViewActivitiesRecords)
            {
                volunteerViewActivityRecord.IsViewOrFavorite = false;
            }
            SaveOne(volunteer);
        }

        /// <summary>
        /// 检查volunteer是否看过该activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public bool CheckIfVolunteerViewActivity(User volunteer, Guid activityId)
        {
            //只有角色为volunteer时才有可能返回true
            if (volunteer.UserRole.Contains(Role.Volunteer))
            {
                foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerViewActivitiesRecords)
                {
                    if (record.ActivityId == activityId)
                    {
                        if (record.IsViewOrFavorite == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查volunteer是否sign in该activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public bool CheckIfVolunteerSignInActivity(User volunteer, Guid activityId)
        {
            bool result = false;
            //只有角色为volunteer时才有可能返回true
            if (volunteer.UserRole.Contains(Role.Volunteer))
            {
                foreach (var a in FindActivitesVolunteerSignedIn(volunteer, "", "StartTime", true, 0, 0))
                {
                    if (a.Id == activityId)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 检查volunteer是否完成该activity
        /// </summary>
        /// <param name="volunteerId"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public bool CheckIfVolunteerCompleteActivity(Guid volunteerId, Activity activity)
        {
            foreach(VolunteerParticipateInActivityRecord record in activity.VolunteerStatus)
            {
                if (record.VolunteerId == volunteerId)
                {
                    if (record.VolunteerStatus == VolunteerStatusInActivity.complete)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查volunteer是否收藏该activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public bool CheckIfVolunteerFavoriteActivity(User volunteer, Guid activityId)
        {
            //只有角色为volunteer时才有可能返回true
            if (volunteer.UserRole.Contains(Role.Volunteer))
            {
                foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
                {
                    if (record.ActivityId == activityId)
                    {
                        if (record.IsViewOrFavorite == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查organizer是否能对activity进行check in、check out、kick out等操作
        /// 只有activity的创建者可以进行操作
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        public bool CheckIfOrganizerCanManageActivity(Activity activity, User organizer)
        {
            if (activity.OrganizerId == organizer.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 志愿者收藏一条activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        public void VolunteerFavoriteActivity(User volunteer, Activity activity)
        {
            //指示是否已有记录
            bool hasRecord = false;
            //如果已有记录则更新
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.ActivityId == activity.Id)
                {
                    if (record.IsViewOrFavorite == true)
                    {
                        return;
                    }
                    else
                    {
                        record.IsViewOrFavorite = true;
                        record.WhenViewOrFavorite = DateTime.Now;
                        hasRecord = true;
                    }
                }
            }
            //否则新加记录
            if (hasRecord == false)
            {
                VolunteerViewOrFavoriteRecord volunteerFavoriteActivity = new VolunteerViewOrFavoriteRecord(activity.Id);
                volunteerFavoriteActivity.IsViewOrFavorite = true;
                volunteerFavoriteActivity.WhenViewOrFavorite = DateTime.Now;
                ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords.Add(volunteerFavoriteActivity);
            }
            activity.VolunteerFavoritedTime++;
            SaveOne(volunteer);
            SaveOne(activity);
            //产生VolunteerFavoriteActivityEvent事件
            EventService.Publish("VolunteerFavoriteActivityEvent", volunteer.Id.ToString() + "," + activity.Id.ToString(), volunteer.Id);
        }

        /// <summary>
        /// 志愿者取消收藏一条activity
        /// </summary>
        /// <param name="volunteer"></param>
        /// <param name="activityId"></param>
        public void VolunteerUnFavoriteActivity(User volunteer, Activity activity)
        {
            //指示是否已有收藏记录
            bool hasRecord = false;
            foreach (var record in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerFavoriteActivitiesRecords)
            {
                if (record.ActivityId == activity.Id)
                {
                    if (record.IsViewOrFavorite == true)
                    {
                        record.IsViewOrFavorite = false;
                    }
                    //说明志愿者已经取消收藏该活动
                    else
                    {
                        throw new Exception("志愿者 " + volunteer.Name + " 未收藏该活动！");
                    }
                    hasRecord = true;
                }
            }
            //如果没有收藏记录，说明志愿者未收藏该活动
            if (hasRecord == false)
            {
                throw new Exception("志愿者 " + volunteer.Name + " 未收藏该活动！");
            }
            if (activity.VolunteerFavoritedTime > 0)
            {
                activity.VolunteerFavoritedTime--;
            }
            SaveOne(volunteer);
            SaveOne(activity);
        }

        /// <summary>
        /// 判断volunteer是否满足activity的badge限制(volunteer sign in 时判断)
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool ActivityValidateBadgeLimit(Activity activity, Guid userId)
        {
            List<string> badgeNames = BadgeService.FindAllUserGrantedBadgeNames(userId);
            bool result = true;
            foreach (List<string> smallList in activity.BadgeLimit.MustGranted)
            {
                //判断user获得的badge中是否包含在每一个小list中
                //badgeNames与smallList有交集即可
                result = result & (badgeNames.Intersect(smallList).Any());

                //如果已经false不需要继续判断
                if (result == false)
                {
                    return false;
                }
            }
            //user获得的badge中不能包含CantGranted中的
            //即badgeNames与CantGranted交集为空
            result = result & (!badgeNames.Intersect(activity.BadgeLimit.CantGranted).Any());
            return result;
        }

        /// <summary>
        /// 根据activity的状态，新建最多4个定时器检查activity的状态
        /// </summary>
        /// <param name="activity"></param>
        public void TimeToCheckStatus(Activity activity)
        {
            //先重置db中的statusInDB, 即get status
            var status = activity.Status;
            //再定时启动线程, 每次都get status，重置db中的statusInDB
            CheckActivityStatus CheckOpenSignInTime = new CheckActivityStatus(activity.Id, activity.OpenSignInTime, this);
            CheckActivityStatus CheckCloseSignInTime = new CheckActivityStatus(activity.Id, activity.CloseSignInTime, this);
            CheckActivityStatus CheckStartTime = new CheckActivityStatus(activity.Id, activity.StartTime, this);
            CheckActivityStatus CheckFinishTime = new CheckActivityStatus(activity.Id, activity.FinishTime, this);
        }

        public void AddProfoile(User user, Role role)
        {
            Profile profile;
            string profileName;
            switch (role)
            {
                case Role.Organization:
                    profileName = user.Name + "OrganizationProfile";
                    profile = new OrganizationProfile(profileName, 500);
                    break;
                case Role.Organizer:
                    profileName = user.Name + "OrganizerProfile";
                    profile = new OrganizerProfile(profileName);
                    break;
                case Role.Volunteer:
                    profileName = user.Name + "VolunteerProfile";
                    profile = new VolunteerProfile(profileName);
                    break;
                default: throw new Exception("角色错误！");
            }
            user.AddProfile(profile);
        }

        public bool CheckUserPassword(string email, string password)
        {
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "Email", email.ToLowerInvariant(), QueryLogic.And);
            queryObject.AppendQuery(QueryOperator.Equal, "Password", MD5Encrypt(password), QueryLogic.And);
            if (entityRepository.Find(queryObject).Any())
                return true;
            else return false;
        }

        /// <summary>
        /// 获得我的总分排名及所有volunteer人数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object MyPointRankOfAllVolunteer(Guid id)
        {
            //计算总人数
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            List<Role> userRole = new List<Role>();
            userRole.Add(Role.Volunteer);
            queryDict.Add("EntityType", "User");
            queryDict.Add("UserRole", userRole);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            long volunteerNumber = entityRepository.FindCountOfResult(queryObject);
            //获得我的排名
            IEnumerable<User> allVolunteers = FindAllVolunteers("_userProfiles.Point.totalPoint", false, 0, 0);
            long myPosition = 0;
            foreach (User volunteer in allVolunteers)
            {
                myPosition++;
                if (id == volunteer.Id)
                {
                    break;
                }
            }
            var result = new
            {
                volunteerNumber = volunteerNumber,
                myPosition = myPosition
            };
            return result;
        }

        /// <summary>
        /// 生成Organization注册时使用的ActionValidation，只有拿到这个ActionValidation才能注册成为Organization
        /// </summary>
        /// <returns>ActionValidation的Id</returns>
        public Guid GenerateOrganizationRegisterActionValidation()
        {
            //过期时间设为10天
            DateTime expireTime = DateTime.Now + new TimeSpan(10, 0, 0, 0);
            ActionValidationModel actionValidation = ActionValidationService.GenerateActionValidate("OrganizationRegister", null, expireTime);
            return actionValidation.Id;
        }

        /// <summary>
        /// 手动从数据库中删除一个活动（目前只能在admindesk中使用）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteActivity(Guid id)
        {
            try
            {
                Dictionary<string, object> queryDict = new Dictionary<string, object>();
                queryDict.Add("_id", id);
                queryDict.Add("EntityType", "Activity");
                QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
                queryObject.AppendQuery(queryDict, QueryLogic.And);
                entityRepository.Delete(queryObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除所有未引用过的文件
        /// 包括1.活动的照片、封面；2.用户头像
        /// </summary>
        /// <param name="staticFilePath">到static文件夹为止的完整路径，如D:/Project/Volunteer/Volunteer.Web/Static</param>
        public string DeleteAllUnReferencedStaticFile(string staticFilePath)
        {
            //活动的照片、封面
            HashSet<Guid> allActivityReferencedFileNames = new HashSet<Guid>();
            foreach (Activity activity in FindAllActivities("", "", false, 0, 0))
            {
                //照片
                foreach (string photo in activity.Photos)
                {
                    allActivityReferencedFileNames.Add(fileNameToGuid(photo));
                }
                //封面
                if (activity.Cover != null)
                {
                    allActivityReferencedFileNames.Add(fileNameToGuid(activity.Cover));
                }
            }
            int deleteActivityFileNumber = deletedUnReferencedFile(allActivityReferencedFileNames, staticFilePath + "/Images/Activity");


            //用户头像
            HashSet<Guid> allUserAvatarReferancedFileName = new HashSet<Guid>();
            foreach (User user in FindAllUsers("", false, 0, 0))
            {
                foreach (Profile userProfile in user.UserProfiles.AllUserProfile)
                {
                    if (userProfile.Avatar.AvatarPath != null)
                    {
                        allUserAvatarReferancedFileName.Add(fileNameToGuid(userProfile.Avatar.AvatarPath));
                    }
                }
            }
            int deleteAvatarFileNumber = deletedUnReferencedFile(allUserAvatarReferancedFileName, staticFilePath + "/Images/Avatar");

            return "删除未引用的文件个数：活动的照片、封面（" + deleteActivityFileNumber + "）用户头像（" + deleteAvatarFileNumber + "）";
        }

        /// <summary>
        /// 从静态文件的路径中获得文件名并转为guid
        /// 如从/Static/Images/Activity/123.jpg中获得文件名123，并转为guid，如果该文件名不是guid，则返回Guid.Enmpty
        /// </summary>
        /// <param name="path">类似/Static/Images/Activity/123.jpg或D:\\Project\\Volunteer\\Volunteer.Web\\Static\\Images\\Activity\\123.jpg的字符串</param>
        /// <returns></returns>
        private Guid fileNameToGuid(string path)
        {
            string fileName;
            if (path.Contains("\\"))
            {
                fileName = path.Split('\\').Last().Split('.').First();
            }
            else
            {
                fileName = path.Split('/').Last().Split('.').First();
            }
            try
            {
                Guid result = new Guid(fileName);
                return result;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 从本地文件中删除未引用的文件
        /// </summary>
        /// <param name="referencedFiles">已经引用的文件名</param>
        /// <param name="staticFilePath">本地文件的文件夹路径如D:/Project/Volunteer/Volunteer.Web/Static/Images/Activity</param>
        /// <returns>删除文件个数</returns>
        private int deletedUnReferencedFile(HashSet<Guid> referencedFiles, string staticFilePath)
        {
            DirectoryInfo staticFileFolder = new DirectoryInfo(staticFilePath);
            //所有本地文件名及完整路径
            Dictionary<Guid, string> allStaticFile = new Dictionary<Guid, string>();            
            foreach (FileInfo file in staticFileFolder.GetFiles())
            {
                //排除文件名为非Guid的文件
                if (fileNameToGuid(file.FullName) != Guid.Empty)
                {
                    allStaticFile.Add(fileNameToGuid(file.FullName), file.FullName);
                }
            }
            //得到未引用的本地文件
            IEnumerable<Guid> unReferrencedFiles = allStaticFile.Keys.Except(referencedFiles);
            //新建已删除文件文件夹，命名方式为staticFilePath后加Delete at和当前时间
            string deletedFile = staticFilePath + "/Delete at " + DateTime.Now.ToString("yyyy-MM-dd HH点mm分");
            Directory.CreateDirectory(deletedFile);
            //将未引用的本地文件移动到已删除文件夹
            foreach (Guid unReferrencedFile in unReferrencedFiles)
            {
                string filePath = allStaticFile[unReferrencedFile];
                //if (File.Exists(filePath))
                //{
                //    File.Delete(filePath);
                //}
                FileInfo file = new FileInfo(filePath);
                file.MoveTo(deletedFile + "/" + file.Name);                
            }
            return unReferrencedFiles.Count();
        }
        
        /// <summary>
        /// 锁定注册超过30天且未加入任何组织的组织者
        /// </summary>
        public void DeleteOrganizerNotJionOrganization()
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("EntityType", "User");
            //organizer
            queryDict.Add("UserRole", Role.Organizer);
            //注册时间在一个月以前的
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$lt", DateTime.UtcNow - new TimeSpan(30, 0, 0, 0));
            queryDict.Add("SignUpTime", subQueryDict1);
            //没有加入任何组织的
            queryDict.Add("_userProfiles.MyOrganizations", new List<int>());
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            foreach(Entity user in entityRepository.Find(queryObject))
            {
                if (((User)user).IsLockedOut == false)
                {
                    ((User)user).IsLockedOut = true;
                    user.Save();
                }
            }
        }

        /// <summary>
        /// 排序以及分页(sortByKey为""或者null不排序；pageIndex和pageSize有一个为0，不分页)
        /// </summary>
        /// <param name="source">总信息</param>
        /// <param name="sortByKey">排序的依据</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">每页元素个数</param>
        /// <returns></returns>
        public List<Activity> SortAndPaging(List<Activity> source, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Activity> result = new List<Activity>();
            if (source.Count > 0)
            {
                //排序
                if (sortByKey != "" && sortByKey != null)
                {
                    Camparer<Activity> reverser = new Camparer<Activity>(typeof(Activity), sortByKey, isAscending);
                    source.Sort(reverser);
                }
                //分页
                if (pageIndex == 0 || pageSize == 0)
                {
                    return source;
                }
                result = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            return result;
        }
        public List<User> SortAndPaging(List<User> source, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<User> result = new List<User>();
            if (source.Count > 0)
            {
                //排序
                if (sortByKey != "" && sortByKey != null)
                {
                    Camparer<User> reverser = new Camparer<User>(typeof(User), sortByKey, isAscending);
                    source.Sort(reverser);
                }
                //分页
                if (pageIndex == 0 || pageSize == 0)
                {
                    return source;
                }
                result = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            return result;
        }
        public List<object> SortAndPaging(List<object> source, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<object> result = new List<object>();
            if (source.Count > 0)
            {
                //排序
                if (sortByKey != "" && sortByKey != null)
                {
                    Camparer<object> reverser = new Camparer<object>(source.FirstOrDefault().GetType(), sortByKey, isAscending);
                    source.Sort(reverser);
                }
                //分页
                if (pageIndex == 0 || pageSize == 0)
                {
                    return source;
                }
                result = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            return result;
        }
        public List<T> SortAndPaging<T>(List<T> source, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<T> result = new List<T>();
            if (source.Count > 0)
            {
                //排序
                if (sortByKey != "" && sortByKey != null)
                {
                    Camparer<T> reverser = new Camparer<T>(source.FirstOrDefault().GetType(), sortByKey, isAscending);
                    source.Sort(reverser);
                }
                //分页
                if (pageIndex == 0 || pageSize == 0)
                {
                    return source;
                }
                result = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            return result;
        }

        /// <summary>
        /// 进行md5加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public string MD5Encrypt(string txt)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(txt));
            return System.Text.Encoding.Default.GetString(result);
        }

        //每小时执行一次
        //每当月份变化重置所有organization的每月point余额, 并生成统计信息
        //每个即将开始的活动都产生ActivityAboutToStartEvent事件
        //删除注册超过30天且未加入任何组织的组织者
        private void checkEveryHour()
        {
            DateTime currentTime = DateTime.Now;
            List<Guid> hasHandledAboutToStartActivityIds = new List<Guid>();
            while (true)
            {
                //每当月份变化重置所有organization的每月point余额, 并生成统计信息
                if (currentTime.Month != DateTime.Now.Month)
                {
                    foreach (User organization in FindAllOrganizations(null, true, 0, 0))
                    {
                        //重置余额
                        ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).RemainingSum = ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).TotalPointEachMonth;
                        //生成统计信息
                        ((OrganizationProfile)organization.UserProfiles[organization.Name + "OrganizationProfile"]).UpdateStatistics(organization, this);
                        organization.Save();
                    }
                }
                currentTime = DateTime.Now;

                //每个24小时之内开始的活动都产生ActivityAboutToStartEvent事件
                List<Activity> aboutToStartActivities = FindAllAboutToStartIn24hActivities().ToList();
                List<Guid> aboutToStartActivityIds = new List<Guid>();
                foreach (Activity activity in aboutToStartActivities)
                {
                    aboutToStartActivityIds.Add(activity.Id);
                }
                //从所有即将开始的活动中排除已经处理过的，再产生事件
                IEnumerable<Guid> notHandledYet = aboutToStartActivityIds.Except(hasHandledAboutToStartActivityIds);
                foreach (Guid activityId in notHandledYet)
                {
                    EventService.Publish("ActivityAboutToStartEvent", null, activityId);
                    hasHandledAboutToStartActivityIds.Add(activityId);
                }

                //锁定注册超过30天且未加入任何组织的组织者
                DeleteOrganizerNotJionOrganization();

                Thread.Sleep(3600000);
                //Thread.Sleep(1000);
            }
        }
        #endregion other

        #region Filter
        private QueryObject<Entity> filterByPoint(long from, long to)
        {
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            //from == to
            if (from == to)
            {
                queryObject.AppendQuery(QueryOperator.Like, "Point", from.ToString(), QueryLogic.And);
            }
            //小于等于to
            else if (from == 0 && to != 0)
            {
                queryObject.AppendQuery(QueryOperator.LessEqual, "Point", to.ToString(), QueryLogic.And);
            }
            //大于等于from
            else if (from != 0 && to == 0)
            {
                queryObject.AppendQuery(QueryOperator.GreaterEqual, "Point", from.ToString(), QueryLogic.And);
            }
            //大于等于from且小于等于to
            else if (from < to)
            {
                queryObject.AppendQuery(QueryOperator.GreaterEqual, "Point", from.ToString(), QueryLogic.And);
                queryObject.AppendQuery(QueryOperator.LessEqual, "Point", to.ToString(), QueryLogic.And);
            }
            //大于等于to且小于等于from
            else
            {
                queryObject.AppendQuery(QueryOperator.GreaterEqual, "Point", to.ToString(), QueryLogic.And);
                queryObject.AppendQuery(QueryOperator.LessEqual, "Point", from.ToString(), QueryLogic.And);
            }
            return queryObject;
        }

        
        /// <summary>
        /// 用于生成查找activity的queryObject
        /// </summary>
        /// <param name="filterSource">形式为：aaa+bbb+ccc+tags:ddd,eee</param>       
        /// <returns></returns>
        private QueryObject<Entity> generateActivityFilter(string filterSource)
        {
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            //过滤filterSource
            if (filterSource != null & filterSource != "")
            {
                //从url中获取的filterSource
                //需要将"%3A"替换为":"
                //"%2C"替换为","
                string filterResult = filterSource.Replace("%3A", ":").Replace("%2C", ",");
                //用'+'分割，前半部分未被标记的为name，用tag:标记的为tag
                string[] strings = filterSource.Split('+');
                List<string> names = new List<string>();
                List<string> tags = new List<string>();
                //将filterSource中的filter信息读出来
                foreach (string str in strings)
                {
                    //tag
                    if (str.IndexOf("tags:") == 0)
                    {
                        string[] tagsString = str.Substring("tags:".Length).Split(',');
                        foreach (string tag in tagsString)
                        {
                            tags.Add(tag);
                        }
                    }
                    //name
                    else
                    {
                        names.Add(str);
                    }
                }
                //构建Name query
                if (names.Any())
                {
                    QueryObject<Entity> nameQueryObject = new QueryObject<Entity>(entityRepository);
                    foreach (string name in names)
                    {
                        nameQueryObject.AppendQuery(QueryOperator.Like, "Name", name, QueryLogic.Or);
                    }
                    queryObject.AppendQuery(nameQueryObject, QueryLogic.And);
                }
                //构建Tag query
                if (tags.Any())
                {
                    Dictionary<string, object> queryDict = new Dictionary<string, object>();
                    Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
                    subQueryDict.Add("$all", tags);
                    queryDict.Add("Tags", subQueryDict);
                    queryObject.AppendQuery(queryDict, QueryLogic.And);
                }
            }
            return queryObject;
        }
        #endregion Filter

        #endregion IVolunteerService
    }

    //自动更改数据库中activity status
    //新建线程，定时启动，每次都get activity status，重置db中的statusInDB
    internal class CheckActivityStatus
    {
        private VolunteerService myService;
        private Guid activityId;
        private DateTime alertTime;
        public CheckActivityStatus(Guid activityId, DateTime alertTime, VolunteerService myService)
        {
            this.activityId = activityId;
            this.alertTime = alertTime;
            this.myService = myService;
            Thread CheckTimeThread = new Thread(this.CheckStatus);
            CheckTimeThread.Start();
        }
        public void CheckStatus()
        {
            TimeSpan dely = new TimeSpan(0, 0, 5);
            TimeSpan timeToGo = alertTime - DateTime.UtcNow + dely;
            if (timeToGo < TimeSpan.Zero)
            {
                return;
            }
            try
            {
                Thread.Sleep(timeToGo);
            }
            catch
            {
                Thread.Sleep(Int32.MaxValue);
            }
            //Activity activity = myService.FindActivity(activityId);
            //var a = activity.Status;
            //System.Diagnostics.Debug.WriteLine(activity.Name + " Status: " + a);
        }
    }

}
