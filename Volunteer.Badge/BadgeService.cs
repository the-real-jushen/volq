using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Repository;
using Jtext103.Volunteer.Badge.Interface;
using Jtext103.Volunteer.VolunteerEvent;


namespace Jtext103.Volunteer.Badge
{
    public class BadgeService
    {
        private static IRepository<BadgeDescription> badgeDescriptionRepository;
        private static IRepository<BadgeEntity> badgeEntityRepository;
        public static void InitService(IRepository<BadgeDescription> badgeDescriptionRepository, IRepository<BadgeEntity> badgeEntityRepository)
        {
            BadgeService.badgeDescriptionRepository = badgeDescriptionRepository;
            BadgeService.badgeEntityRepository = badgeEntityRepository;
        }

        /// <summary>
        /// 通过badgeName查找BadgeDescription
        /// </summary>
        /// <param name="badgeName"></param>
        /// <returns></returns>
        public static BadgeDescription FindBadgeDescriptionByName(string badgeName)
        {
            QueryObject<BadgeDescription> queryObject = new QueryObject<BadgeDescription>(badgeDescriptionRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeName, QueryLogic.And);
            var result = badgeDescriptionRepository.Find(queryObject);
            if (result.Count() == 1)
            {
                return result.FirstOrDefault();
            }
            else if (result.Count() == 0)
            {
                return null;
            }
            else
            {
                throw new Exception(badgeName + " badge不唯一！");
            }
        }

        /// <summary>
        /// 获得所有badge的name
        /// </summary>
        /// <returns></returns>
        public static List<BadgeDescription> FindAllBadges()
        {
            return FindAllBadges("BadgeName", false, 0, 0);
        }
        public static List<BadgeDescription> FindAllBadges(string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            List<BadgeDescription> allBadges = badgeDescriptionRepository.FindAll(sortByKey, isAscending, pageIndex, pageSize).ToList<BadgeDescription>();
            return allBadges;
        }

        /// <summary>
        /// 获得所有badge的个数
        /// </summary>
        /// <returns></returns>
        public static long FindAllBadgeCount()
        {
            return badgeDescriptionRepository.FindAllCount();
        }

        /// <summary>
        /// 找到该用户所有已经获得的badge name
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<string> FindAllUserGrantedBadgeNames(Guid userId)
        {
            return FindAllUserGrantedBadgeNames(userId, "GrantedTime", false, 0, 0);
        }
        public static List<string> FindAllUserGrantedBadgeNames(Guid userId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryDic.Add("IsGranted", true);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            var badgeEntities = badgeEntityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            List<string> result = new List<string>();
            foreach (BadgeEntity badgeEntity in badgeEntities)
            {
                result.Add(badgeEntity.BadgeName);
            }
            return result;
        }

        /// <summary>
        /// 找到该用户所有已经获得的BadgeEntity
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<BadgeEntity> FindAllUserGrantedBadgeEntity(Guid userId)
        {
            return FindAllUserGrantedBadgeEntity(userId, "GrantedTime", false, 0, 0);
        }
        public static List<BadgeEntity> FindAllUserGrantedBadgeEntity(Guid userId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryDic.Add("IsGranted", true);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            List<BadgeEntity> badgeEntities = badgeEntityRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize).ToList();
            return badgeEntities;
        }

        /// <summary>
        /// 该用户所有已经获得的badge的个数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static long FindAllUserGrantedBadgeCount(Guid userId)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryDic.Add("IsGranted", true);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            return badgeEntityRepository.FindCountOfResult(queryObject);
        }

        /// <summary>
        /// 向数据库中添加所有的BadgeDescriptions
        /// </summary>
        /// <param name="badgeHandlers"></param>
        public static void RegisterBadgeDescriptions(List<IBadge> badgeHandlers)
        {
            foreach (var badgeHandler in badgeHandlers)
            {
                var badgeDescriptions = badgeHandler.GetBadgeDescription();
                //for each badges
                //if the badge name esists then skip
                foreach (BadgeDescription badgeDescription in badgeDescriptions)
                {
                    QueryObject<BadgeDescription> queryObject = new QueryObject<BadgeDescription>(badgeDescriptionRepository);
                    queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeDescription.BadgeName, QueryLogic.And);
                    var result = badgeDescriptionRepository.Find(queryObject);
                    if (!result.Any())
                    {
                        badgeDescriptionRepository.SaveOne(badgeDescription);
                    }
                }
            }
        }

        /// <summary>
        /// 检查volunteer是否获得badge
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="badgeName"></param>
        /// <returns></returns>
        public static bool CheckIfBadgeGranted(Guid userId, string badgeName)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeName, QueryLogic.And);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryDic.Add("IsGranted", true);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            var result = badgeEntityRepository.Find(queryObject);
            if (result.Count() == 1)
            {
                return true;
            }
            else if (result.Count() == 0)
            {
                return false;
            }
            else
            {
                throw new Exception(badgeName + " badge不唯一！");
            }
        }

        /// <summary>
        /// the requiment is satisfied, call this to set the requirement to satisfied.
        /// it will create badge entity if neccessary,
        /// it will grant badge it all requirements are met
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="badgeName"></param>
        /// <param name="requirement"></param>
        /// <param name="badgeDesc"></param>
        /// <returns>if the badge has been just granted</returns>
        public static bool SatisfyRequirement(Guid userId, string badgeName, string requirement, BadgeDescription badgeDesc)
        {
            //first find if there is a badge entity
            //if not create one and mark the requirement as satisfied
            //if badge entity found, check if the requirment is met
            //if not set to met
            //if all the requirement is satisfied, grant the badge
            //generate a badge granted event
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeName, QueryLogic.And);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            var result = badgeEntityRepository.Find(queryObject);
            //数据库中存在该BadgeEntity，则修改
            if (result.Count() == 1)
            {
                BadgeEntity badgeEntity = result.FirstOrDefault();
                //redundance code
                //如果已经获得，则直接返回
                if (badgeEntity.IsGranted == true)
                {
                    return false;
                }
                badgeEntity.WetherRequirementSatisfaction[requirement] = true;
                //重新计算是否能得到这个徽章（是否满足所有需求）
                bool isGranted = true;
                foreach (bool trueOrFalse in badgeEntity.WetherRequirementSatisfaction.Values)
                {
                    isGranted = isGranted & trueOrFalse;
                }
                badgeEntity.IsGranted = isGranted;
                if (isGranted == true)
                {
                    //获得该badge的时间为DateTime.Now
                    badgeEntity.GrantedTime = DateTime.Now;
                    //产生BadgeGrantedEvent事件
                    EventService.Publish("BadgeGrantedEvent", badgeEntity.BadgeName, badgeEntity.UserId);
                }
                badgeEntityRepository.SaveOne(badgeEntity);
                return badgeEntity.IsGranted;
            }
            //数据库中不存在该BadgeEntity，则新建
            else if (result.Count() == 0)
            {
                BadgeEntity badgeEntity = new BadgeEntity(userId, badgeName);
                foreach (string key in badgeDesc.RequirementDescription.Keys)
                {
                    if (key == requirement)
                    {
                        badgeEntity.WetherRequirementSatisfaction.Add(key, true);
                    }
                    else
                    {
                        badgeEntity.WetherRequirementSatisfaction.Add(key, false);
                    }
                }
                //重新计算是否能得到这个徽章（是否满足所有需求）
                bool isGranted = true;
                foreach (bool trueOrFalse in badgeEntity.WetherRequirementSatisfaction.Values)
                {
                    isGranted = isGranted & trueOrFalse;
                }
                badgeEntity.IsGranted = isGranted;
                if (isGranted == true)
                {
                    //获得该badge的时间为DateTime.Now
                    badgeEntity.GrantedTime = DateTime.Now;
                    //产生BadgeGrantedEvent事件
                    EventService.Publish("BadgeGrantedEvent", badgeEntity.BadgeName, badgeEntity.UserId);
                }
                badgeEntityRepository.SaveOne(badgeEntity);
                return badgeEntity.IsGranted;
            }
            else
            {
                throw new Exception(badgeName + " badge不唯一！");
            }
        }

        /// <summary>
        /// 手动（强制）颁发徽章
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="badgeName"></param>
        /// <returns></returns>
        public static bool GrantBadge(Guid userId, string badgeName)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeName, QueryLogic.And);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            var result = badgeEntityRepository.Find(queryObject);
            //数据库中存在该BadgeEntity，则修改
            if (result.Count() == 1)
            {
                BadgeEntity badgeEntity = result.FirstOrDefault();
                //如果已经获得，则直接返回
                if (badgeEntity.IsGranted == true)
                {
                    return false;
                }
                else
                {
                    badgeEntity.IsGranted = true;
                    badgeEntity.GrantedTime = DateTime.Now;
                    badgeEntityRepository.SaveOne(badgeEntity);
                    //产生BadgeGrantedEvent事件
                    EventService.Publish("BadgeGrantedEvent", badgeEntity.BadgeName, badgeEntity.UserId);
                    return true;
                }
            }
            //数据库中不存在该BadgeEntity，则新建
            else if (result.Count() == 0)
            {
                BadgeEntity badgeEntity = new BadgeEntity(userId, badgeName);
                badgeEntity.IsGranted = true;
                badgeEntity.GrantedTime = DateTime.Now;
                badgeEntityRepository.SaveOne(badgeEntity);
                //产生BadgeGrantedEvent事件
                EventService.Publish("BadgeGrantedEvent", badgeEntity.BadgeName, badgeEntity.UserId);
                return true;
            }
            else
            {
                throw new Exception(badgeName + " badge不唯一！");
            }
        }

        /// <summary>
        /// 手动（强制）收回徽章
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="badgeName"></param>
        /// <returns></returns>
        public static bool UngrantBadge(Guid userId, string badgeName)
        {
            QueryObject<BadgeEntity> queryObject = new QueryObject<BadgeEntity>(badgeEntityRepository);
            queryObject.AppendQuery(QueryOperator.Equal, "BadgeName", badgeName, QueryLogic.And);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("UserId", userId);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            var result = badgeEntityRepository.Find(queryObject);
            //数据库中存在该BadgeEntity，则修改
            if (result.Count() == 1)
            {
                BadgeEntity badgeEntity = result.FirstOrDefault();
                //如果未获得，则直接返回
                if (badgeEntity.IsGranted != true)
                {
                    return false;
                }
                else
                {
                    badgeEntity.IsGranted = false;
                    badgeEntity.GrantedTime = DateTime.Now;
                    badgeEntityRepository.SaveOne(badgeEntity);
                    return true;
                }
            }
            //数据库中不存在该BadgeEntity，则直接返回
            else if (result.Count() == 0)
            {
                return true;
            }
            else
            {
                throw new Exception(badgeName + " badge不唯一！");
            }
        }

    }
}
