using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.Friend;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Repository;
using Jtext103.Repository.Interface;
using Jtext103.Volunteer.VolunteerEvent;
using Jtext103.Volunteer.Badge;


namespace Jtext103.Volunteer.Service
{
    public class FriendServiceInVolunteerService
    {
        private IRepository<Entity> entityRepository;
        private VolunteerService volunteerService;
        public FriendServiceInVolunteerService(IRepository<Entity> entityRepository, VolunteerService volunteerService)
        {
            this.entityRepository = entityRepository;
            this.volunteerService = volunteerService;
        }

        /// <summary>
        /// 找到给定id的volunteer的好友
        /// </summary>
        public List<User> MyFriends(Guid id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            IEnumerable<Guid> myfriendIds = FriendService.FindMyFriends(id);
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", myfriendIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 对我和我的好友进行排名
        /// </summary>
        /// <param name="id">我的id</param>
        /// <param name="sortByKey">只能为"","point","activityCount","badgeCount"中的一个</param>
        /// <param name="isAscending"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<VolunteerAndFriendsRankModel> MeAndMyFriendsRank(Guid id, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<Guid> meAndMyfriendIds = FriendService.FindMyFriends(id);
            meAndMyfriendIds.Add(id);
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", meAndMyfriendIds);
            queryDict.Add("_id", subQueryDict);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            List<VolunteerAndFriendsRankModel> result = new List<VolunteerAndFriendsRankModel>();
            if (sortByKey == "point")
            {
                IEnumerable<Entity> orderByPoint = entityRepository.Find(queryObject, "_userProfiles.Point.totalPoint", isAscending, pageIndex, pageSize);
                foreach (User volunteer in orderByPoint)
                {
                    var a = new VolunteerAndFriendsRankModel
                    {
                        id = volunteer.Id,
                        email = volunteer.Email,
                        name = volunteer.Name,
                        avatar = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Avatar.AvatarPath,
                        affiliation = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Affiliation,
                        point = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Point.TotalPoint,
                        activityCount = volunteerService.FindAllVolunteerCompletedActivities(volunteer, "", false, 0, 0).Count(),
                        badgeCount = BadgeService.FindAllUserGrantedBadgeCount(volunteer.Id),
                        level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel,
                        levelName = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevelName,
                        levelPicture = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevelPicture
                    };
                    result.Add(a);
                }
            }
            else if (sortByKey == "" || sortByKey == "activityCount" || sortByKey == "badgeCount")
            {
                IEnumerable<Entity> meAndMyFriends = entityRepository.Find(queryObject);
                List<VolunteerAndFriendsRankModel> source = new List<VolunteerAndFriendsRankModel>();
                foreach (User volunteer in meAndMyFriends)
                {
                    var a = new VolunteerAndFriendsRankModel
                    {
                        id = volunteer.Id,
                        email = volunteer.Email,
                        name = volunteer.Name,
                        avatar = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Avatar.AvatarPath,
                        affiliation = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Affiliation,
                        point = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).Point.TotalPoint,
                        activityCount = volunteerService.FindAllVolunteerCompletedActivities(volunteer, "", false, 0, 0).Count(),
                        badgeCount = BadgeService.FindAllUserGrantedBadgeCount(volunteer.Id),
                        level = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevel,
                        levelName = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevelName,
                        levelPicture = ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).VolunteerLevelPicture
                    };
                    source.Add(a);
                }
                if (sortByKey == "")
                {
                    result = source;
                }
                if (sortByKey == "activityCount")
                {
                    result = volunteerService.SortAndPaging<VolunteerAndFriendsRankModel>(source, "activityCount", isAscending, pageIndex, pageSize);
                }
                if (sortByKey == "badgeCount")
                {
                    result = volunteerService.SortAndPaging<VolunteerAndFriendsRankModel>(source, "badgeCount", isAscending, pageIndex, pageSize);
                }
            }
            else
            {
                throw new Exception("sortByKey参数错误");
            }
            return result;
        }

        /// <summary>
        /// 获得我的排名
        /// </summary>
        /// <param name="id">我的id</param>
        /// <param name="sortByKey">只能为"","point","activityCount","badgeCount"中的一个</param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        public int GetMyRank(Guid id, string sortByKey, bool isAscending)
        {
            var source = MeAndMyFriendsRank(id, sortByKey, isAscending, 0, 0);
            int myRanking = 0;
            foreach (var model in source)
            {
                myRanking++;
                if (model.id == id)
                {
                    break;
                }
            }
            return myRanking;
        }

        /// <summary>
        /// 申请添加好友
        /// </summary>
        /// <param name="apply">申请人</param>
        /// <param name="beApplied">被申请人</param>
        /// <param name="comment">申请理由</param>
        /// <returns></returns>
        public bool ApplyFriend(User apply, User beApplied, string comment)
        {
            //两个人已经是好友，无法申请
            if (FriendService.CheckIfWeAreFriends(apply.Id, beApplied.Id))
            {
                return false;
            }
            //申请人(apply)已申请过(beApplied)的好友，且状态为applying，则无法申请
            foreach (var information in ((VolunteerProfile)apply.UserProfiles[apply.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == beApplied.Id) && (information.Status == ApplyFriendStatus.Applying))
                {
                    return false;
                }
            }
            //被申请人(beApplied)已申请过(apply)的好友，且状态为applying，则无法申请
            foreach (var information in ((VolunteerProfile)beApplied.UserProfiles[beApplied.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == apply.Id) && (information.Status == ApplyFriendStatus.Applying))
                {
                    return false;
                }
            }

            ((VolunteerProfile)apply.UserProfiles[apply.Name + "VolunteerProfile"]).ApplyFriendFromMe.Add(new ApplyFriendFromMe(beApplied.Id, beApplied.Name));
            ((VolunteerProfile)beApplied.UserProfiles[beApplied.Name + "VolunteerProfile"]).ApplyFriendToMe.Add(new ApplyFriendToMe(apply.Id, apply.Name, comment));
            volunteerService.SaveOne(apply);
            volunteerService.SaveOne(beApplied);
            //产生申请好友事件
            EventService.Publish("ApplyFriendEvent", apply.Id.ToString() + "," + beApplied.Id.ToString() + "," + comment, apply.Id);
            return true;
        }

        /// <summary>
        /// 同意好友申请并添加好友
        /// </summary>
        /// <param name="apply">申请人</param>
        /// <param name="beApplied">被申请人（同意好友申请的人）</param>
        /// <param name="comment">接受理由</param>
        /// <returns></returns>
        public bool AcceptFriendApplication(User apply, User beApplied, string comment)
        {
            //两个人已经是好友，则报错
            if (FriendService.CheckIfWeAreFriends(apply.Id, beApplied.Id))
            {
                return false;
            }
            //被申请人的profile中ApplyFriendToMe的hasHandled改为true
            foreach (var information in ((VolunteerProfile)beApplied.UserProfiles[beApplied.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if ((information.VolunteerId == apply.Id) && (information.hasHandled == false))
                {
                    information.hasHandled = true;
                    break;
                }
            }
            //申请人的profile中ApplyFriendFromMe的申请的记录修改为accept
            foreach (var information in ((VolunteerProfile)apply.UserProfiles[apply.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == beApplied.Id) && (information.Status == ApplyFriendStatus.Applying))
                {
                    information.Status = ApplyFriendStatus.Accept;
                    information.ActionTime = DateTime.Now;
                    information.Comment = comment;
                    break;
                }
            }
            //在数据库中添加两人好友信息
            if (FriendService.MakeFriend(apply.Id, beApplied.Id) == true)
            {
                volunteerService.SaveOne(apply);
                volunteerService.SaveOne(beApplied);
                //产生同意好友申请事件
                EventService.Publish("AcceptFriendApplicationEvent", apply.Id.ToString() + "," + beApplied.Id.ToString() + "," + comment, beApplied.Id);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="apply">申请人</param>
        /// <param name="beApplied">被申请人（拒绝好友申请的人）</param>
        /// <param name="comment">拒绝理由</param>
        /// <returns></returns>
        public bool RefuseFriendApplication(User apply, User beApplied, string comment)
        {
            //两个人已经是好友，则报错
            if (FriendService.CheckIfWeAreFriends(apply.Id, beApplied.Id))
            {
                return false;
            }
            //被申请人的profile中ApplyFriendToMe的hasHandled改为true
            foreach (var information in ((VolunteerProfile)beApplied.UserProfiles[beApplied.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if ((information.VolunteerId == apply.Id) && (information.hasHandled == false))
                {
                    information.hasHandled = true;
                    break;
                }
            }
            //申请人的profile中ApplyFriendFromMe的申请的记录修改为refuse
            foreach (var information in ((VolunteerProfile)apply.UserProfiles[apply.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == beApplied.Id) && (information.Status == ApplyFriendStatus.Applying))
                {
                    information.Status = ApplyFriendStatus.Refuse;
                    information.ActionTime = DateTime.Now;
                    information.Comment = comment;
                    break;
                }
            }
            volunteerService.SaveOne(apply);
            volunteerService.SaveOne(beApplied);
            //产生拒绝好友申请事件
            EventService.Publish("RefuseFriendApplicationEvent", apply.Id.ToString() + "," + beApplied.Id.ToString() + "," + comment, beApplied.Id);
            return true;
        }

        /// <summary>
        /// 断绝好友关系
        /// </summary>
        /// <param name="apply">申请断绝关系的人</param>
        /// <param name="beApplied">被申请人</param>
        /// <returns></returns>
        public bool BreakOffFriendship(User apply, User beApplied)
        {
            //如果两人不是好友，则报错
            if (!FriendService.CheckIfWeAreFriends(apply.Id, beApplied.Id))
            {
                return false;
            }
            //如果当时是apply这个user申请的好友，则将apply的profile中的ApplyFriendFromMe的申请记录修改为breakoff
            foreach (var information in ((VolunteerProfile)apply.UserProfiles[apply.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == beApplied.Id) && (information.Status == ApplyFriendStatus.Accept))
                {
                    information.Status = ApplyFriendStatus.BreakOff;
                    information.ActionTime = DateTime.Now;
                    information.Comment = "你主动断绝了好友关系！";
                    break;
                }
            }
            //如果当时是beApplied这个user申请的好友，则将beApplied的profile中的ApplyFriendFromMe的申请记录修改为breakoff
            foreach (var information in ((VolunteerProfile)beApplied.UserProfiles[beApplied.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if ((information.VolunteerId == apply.Id) && (information.Status == ApplyFriendStatus.Accept))
                {
                    information.Status = ApplyFriendStatus.BreakOff;
                    information.ActionTime = DateTime.Now;
                    information.Comment = "对方和你断绝了好友关系！";
                    break;
                }
            }
            //数据库中清除两人的好友信息
            if (FriendService.BreakOffFriendship(apply.Id, beApplied.Id))
            {
                volunteerService.SaveOne(apply);
                volunteerService.SaveOne(beApplied);
                //产生断绝好友关系事件
                EventService.Publish("BreakOffFriendshipEvent", apply.Id.ToString() + "," + beApplied.Id.ToString(), apply.Id);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取所有正在向我申请好友的volunteer
        /// </summary>
        /// <param name="volunteer"></param>
        /// <returns></returns>
        public List<User> AllUserApplyingFriendToMe(User volunteer)
        {
            List<Guid> allUserApplying = new List<Guid>();
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if (information.hasHandled == false)
                {
                    allUserApplying.Add(information.VolunteerId);
                }
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$in", allUserApplying);
            queryDict.Add("_id", subQueryDict1);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 获取所有我向其他人申请好友的记录
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public List<User> AllUserApplyingFriendFromMe(User volunteer)
        {
            List<Guid> allMyApplying = new List<Guid>();
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                allMyApplying.Add(information.VolunteerId);
            }
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$in", allMyApplying);
            queryDict.Add("_id", subQueryDict1);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 获取所有我能申请好友的volunteer（既不是好友，也没有在好友申请中的所有volunteer）
        /// </summary>
        /// <param name="volunteer"></param>
        /// <returns></returns>
        public List<User> AllICanApplyFriendUsers(User volunteer)
        {
            //所有我不能申请好友的列表
            //包括1.所有正在向我申请好友的volunteer；2.所有我正在申请的好友的volunteer；3.所有我的好友；4.我自己
            List<Guid> cantApply = new List<Guid>();
            //所有正在向我申请好友的volunteer
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if (information.hasHandled == false)
                {
                    cantApply.Add(information.VolunteerId);
                }
            }
            //所有我正在申请的好友的volunteer
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if (information.Status == ApplyFriendStatus.Applying)
                {
                    cantApply.Add(information.VolunteerId);
                }
            }
            //所有我的好友
            cantApply.AddRange(FriendService.FindMyFriends(volunteer.Id));
            //我自己
            cantApply.Add(volunteer.Id);

            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$nin", cantApply);
            queryDict.Add("_id", subQueryDict1);
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("$in", new List<Role>() { Role.Volunteer });
            queryDict.Add("UserRole", subQueryDict2);
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 为给定id的volunteer推荐指定个数的好友（如果指定个数过多，则返回除给定volunteer的所有volunteer）
        /// </summary>
        /// <param name="myId"></param>
        /// <param name="number">推荐好友的个数</param>
        /// <returns></returns>
        public List<User> RecommendFriends(User volunteer, int number)
        {
            List<User> all = AllICanApplyFriendUsers(volunteer);
            int max = all.Count;
            if (number >= max)
            {
                return all;
            }
            else
            {
                List<User> result = new List<User>();
                for (int i = 0; i < number; i++)
                {
                    Random ran = new Random();
                    int randKey = ran.Next(0, all.Count - 1);
                    result.Add(all[randKey]);
                    //把推荐过的好友删除
                    all.RemoveAt(randKey);
                }
                return result;
            }
        }

        /// <summary>
        /// 通过email（精确查找）、名字（模糊查找）、从属（模糊查找）
        /// 找到我的好友
        /// </summary>
        /// <param name="myId">我的id</param>
        /// <param name="friendName">好友名字</param>
        /// <returns></returns>
        public List<User> SearchMyFriendByFilter(Guid myId, string email, string friendName, string affiliation)
        {
            if ((email == "" || email == null) && (friendName == "" || friendName == null) && (affiliation == "" || affiliation == null))
            {
                return new List<User>();
            }
            IEnumerable<Guid> myfriendIds = FriendService.FindMyFriends(myId);
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict = new Dictionary<string, object>();
            subQueryDict.Add("$in", myfriendIds);
            queryDict.Add("_id", subQueryDict);

            QueryObject<Entity> queryObject = generateQueryObject(email, friendName, affiliation);
            queryObject.AppendQuery(queryDict, QueryLogic.And);

            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 通过email（精确查找）、名字（模糊查找）、从属（模糊查找）
        /// 找到还不是我的好友的volunteer
        /// </summary>
        /// <param name="myId">我的id</param>
        /// <param name="friendName">对方名字</param>
        /// <returns></returns>
        public List<User> SearchNotMyFriendByFilter(Guid myId, string email, string friendName, string affiliation)
        {
            if ((email == "" || email == null) && (friendName == "" || friendName == null) && (affiliation == "" || affiliation == null))
            {
                return new List<User>();
            }
            User volunteer = volunteerService.FindUser(myId);
            //所有我不能申请好友的列表
            //包括1.所有正在向我申请好友的volunteer；2.所有我正在申请的好友的volunteer；3.所有我的好友；4.我自己
            List<Guid> cantApply = new List<Guid>();
            //所有正在向我申请好友的volunteer
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendToMe)
            {
                if (information.hasHandled == false)
                {
                    cantApply.Add(information.VolunteerId);
                }
            }
            //所有我正在申请的好友的volunteer
            foreach (var information in ((VolunteerProfile)volunteer.UserProfiles[volunteer.Name + "VolunteerProfile"]).ApplyFriendFromMe)
            {
                if (information.Status == ApplyFriendStatus.Applying)
                {
                    cantApply.Add(information.VolunteerId);
                }
            }
            //所有我的好友
            cantApply.AddRange(FriendService.FindMyFriends(myId));
            //我自己
            cantApply.Add(myId);

            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            Dictionary<string, object> subQueryDict1 = new Dictionary<string, object>();
            subQueryDict1.Add("$nin", cantApply);
            queryDict.Add("_id", subQueryDict1);
            Dictionary<string, object> subQueryDict2 = new Dictionary<string, object>();
            subQueryDict2.Add("$in", new List<Role>() { Role.Volunteer });
            queryDict.Add("UserRole", subQueryDict2);

            QueryObject<Entity> queryObject = generateQueryObject(email, friendName, affiliation);
            queryObject.AppendQuery(queryDict, QueryLogic.And);

            IEnumerable<Entity> source = entityRepository.Find(queryObject);
            List<User> result = volunteerService.switchToUserList(source);
            return result;
        }

        /// <summary>
        /// 将email（精确查找）、名字（模糊查找）、从属（模糊查找）转为QueryObject
        /// </summary>
        /// <param name="email"></param>
        /// <param name="friendName"></param>
        /// <param name="affiliation"></param>
        private QueryObject<Entity> generateQueryObject(string email, string friendName, string affiliation)
        {
            QueryObject<Entity> queryObject = new QueryObject<Entity>(entityRepository);
            if (email != null && email != "")
            {
                queryObject.AppendQuery(QueryOperator.Equal, "Email", email.ToLowerInvariant(), QueryLogic.And);
            }
            if (friendName != null && friendName != "")
            {
                queryObject.AppendQuery(QueryOperator.Like, "Name", friendName, QueryLogic.And);
            }
            if (affiliation != null && affiliation != "")
            {
                queryObject.AppendQuery(QueryOperator.Like, "_userProfiles.Affiliation", affiliation, QueryLogic.And);
            }
            return queryObject;
        }
    }
    public class VolunteerAndFriendsRankModel
    {
        public Guid id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public List<string> affiliation { get; set; }
        public double point { get; set; }
        public int activityCount { get; set; }
        public long badgeCount { get; set; }
        public int level { get; set; }
        public string levelName { get; set; }
        public string levelPicture { get; set; }
        public VolunteerAndFriendsRankModel()
        {
            affiliation = new List<string>();
        }
    }
}