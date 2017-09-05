using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Repository;

namespace Jtext103.Volunteer.Friend
{
    public class FriendService
    {
        private static IRepository<FriendRelationshipEntity> friendRelationshipEntityRepository;
        public static void InitService(IRepository<FriendRelationshipEntity> friendRelationshipEntityRepository)
        {
            FriendService.friendRelationshipEntityRepository = friendRelationshipEntityRepository;
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="volunteer1Id"></param>
        /// <param name="volunteer2Id"></param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool MakeFriend(Guid volunteer1Id, Guid volunteer2Id)
        {
            //无法加自己好友
            if (volunteer1Id == volunteer2Id)
            {
                return false;
            }
            //已经是好友也无法再加好友
            if (!CheckIfWeAreFriends(volunteer1Id, volunteer2Id))
            {
                FriendRelationshipEntity newRelationshipEnity = new FriendRelationshipEntity(volunteer1Id, volunteer2Id);
                friendRelationshipEntityRepository.SaveOne(newRelationshipEnity);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 断绝好友关系（将两个人的好友信息从数据库中删除）
        /// </summary>
        /// <param name="volunteer1Id"></param>
        /// <param name="volunteer2Id"></param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool BreakOffFriendship(Guid volunteer1Id, Guid volunteer2Id)
        {
            if (CheckIfWeAreFriends(volunteer1Id, volunteer2Id))
            {
                QueryObject<FriendRelationshipEntity> queryObject = new QueryObject<FriendRelationshipEntity>(friendRelationshipEntityRepository);
                Dictionary<string, object> queryDic1 = new Dictionary<string, object>();
                queryDic1.Add("Volunteer1Id", volunteer1Id);
                queryDic1.Add("Volunteer2Id", volunteer2Id);
                Dictionary<string, object> queryDic2 = new Dictionary<string, object>();
                queryDic2.Add("Volunteer1Id", volunteer2Id);
                queryDic2.Add("Volunteer2Id", volunteer1Id);
                queryObject.AppendQuery(queryDic1, QueryLogic.Or);
                queryObject.AppendQuery(queryDic2, QueryLogic.Or);
                friendRelationshipEntityRepository.Delete(queryObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查两个volunteer是否是好友
        /// </summary>
        /// <param name="volunteer1Id"></param>
        /// <param name="volunteer2Id"></param>
        /// <returns></returns>
        public static bool CheckIfWeAreFriends(Guid volunteer1Id, Guid volunteer2Id)
        {
            QueryObject<FriendRelationshipEntity> queryObject = new QueryObject<FriendRelationshipEntity>(friendRelationshipEntityRepository);
            Dictionary<string, object> queryDic1 = new Dictionary<string, object>();
            queryDic1.Add("Volunteer1Id", volunteer1Id);
            queryDic1.Add("Volunteer2Id", volunteer2Id);
            Dictionary<string, object> queryDic2 = new Dictionary<string, object>();
            queryDic2.Add("Volunteer1Id", volunteer2Id);
            queryDic2.Add("Volunteer2Id", volunteer1Id);
            queryObject.AppendQuery(queryDic1, QueryLogic.Or);
            queryObject.AppendQuery(queryDic2, QueryLogic.Or);
            var result = friendRelationshipEntityRepository.Find(queryObject);
            if (result.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 通过自己的id，找到我的所有好友id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Guid> FindMyFriends(Guid id)
        {
            QueryObject<FriendRelationshipEntity> queryObject = new QueryObject<FriendRelationshipEntity>(friendRelationshipEntityRepository);
            Dictionary<string, object> queryDic1 = new Dictionary<string, object>();
            queryDic1.Add("Volunteer1Id", id);
            Dictionary<string, object> queryDic2 = new Dictionary<string, object>();
            queryDic2.Add("Volunteer2Id", id);
            queryObject.AppendQuery(queryDic1, QueryLogic.Or);
            queryObject.AppendQuery(queryDic2, QueryLogic.Or);
            var friendRelationshipEntities = friendRelationshipEntityRepository.Find(queryObject);
            List<Guid> result = new List<Guid>();
            foreach (FriendRelationshipEntity friendRelationshipEntity in friendRelationshipEntities)
            {
                if (friendRelationshipEntity.Volunteer1Id == id)
                {
                    result.Add(friendRelationshipEntity.Volunteer2Id);
                    continue;
                }
                if (friendRelationshipEntity.Volunteer2Id == id)
                {
                    result.Add(friendRelationshipEntity.Volunteer1Id);
                }
            }
            return result;
        }
    }
}
