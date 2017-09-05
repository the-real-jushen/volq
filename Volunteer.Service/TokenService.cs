using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Repository;
using Jtext103.Repository.Interface;

namespace Jtext103.Volunteer.Service
{
    public class TokenService
    {
        public static TokenService Instance;
        private IRepository<TokenModel> repository;
        public TokenService(IRepository<TokenModel> someRepository)
        {
            repository = someRepository;
            Instance = this;
        }
        public TokenModel FindTokenModelByToken(Guid id)
        {
            TokenModel tokenModel;
            tokenModel = (TokenModel)repository.FindOneById(id);
            return tokenModel;
        }
        public void DeleteToken(TokenModel tokenModel)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("_id", tokenModel.Id);
            QueryObject<TokenModel> queryObject = new QueryObject<TokenModel>(repository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            repository.Delete(queryObject);
        }
        public void DeleteToken(Guid userId)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("UserId", userId);
            QueryObject<TokenModel> queryObject = new QueryObject<TokenModel>(repository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            repository.Delete(queryObject);
        }
        public void InsertToken(TokenModel tokenModel)
        {
            repository.InsertOne(tokenModel);
        }
        /// <summary>
        /// 用户登陆时刷新token或新建token
        /// 如果该用户已有token，则刷新过期日期（如果token过期，则删除该token并新建）
        /// 否则新建token
        /// </summary>
        public TokenModel RefreshTokenWhenLogIn(Guid userId, DateTime dueTime)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("UserId", userId);
            QueryObject<TokenModel> queryObject = new QueryObject<TokenModel>(repository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<TokenModel> myTokens = repository.Find(queryObject);
            //只有一个token时
            //如果没有过期则只刷新过期日期
            //如果已经过期，则删除该token并新建
            if (myTokens.Count() == 1)
            {
                TokenModel myToken = myTokens.FirstOrDefault();
                //没有过期，刷新过期日期
                if (ValidateToken(myToken.Id) != null)
                {
                    myToken.DueTime = dueTime;
                    repository.SaveOne(myToken);
                    return myToken;
                }
                //已过期，删除并新建
                else
                {
                    //如果token已过期，调用ValidateToken()时会自动删除该token
                    TokenModel newToken = new TokenModel(userId, dueTime);
                    repository.InsertOne(newToken);
                    return newToken;
                }
            }
            //没有token时，新建token
            else if(myTokens.Count() == 0)
            {
                TokenModel newToken = new TokenModel(userId, dueTime);
                repository.InsertOne(newToken);
                return newToken;
            }
            //有多个token（一般不会出现）
            //先删除该user所有token，再新建一个token
            else
            {
                DeleteToken(userId);
                TokenModel newToken = new TokenModel(userId, dueTime);
                repository.InsertOne(newToken);
                return newToken;
            }
        }
        /// <summary>
        /// 确定给定的token是否过期
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>找到改token且没过期则返回该TokenModel；如果找到token但已过期则在数据库中删除该tokenModel且返回null；如果找不到则返回null；</returns>
        public TokenModel ValidateToken(Guid id)
        {
            TokenModel tokenModel = FindTokenModelByToken(id);
            if (tokenModel != null)
            {
                //validate >=0, 未过期
                //validate < 0, 已过期
                int validate = DateTime.Compare(tokenModel.DueTime.ToLocalTime(), DateTime.Now);
                if (validate >= 0)
                    return tokenModel;
                else
                {
                    DeleteToken(tokenModel);
                    return null;
                }
            }
            else return null;
        }

    }
}
    