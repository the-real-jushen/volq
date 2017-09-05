using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Models;
using Jtext103.Repository;
using Jtext103.Repository.Interface;

namespace Jtext103.Volunteer.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidationService
    {
        private static TokenService tokenServ;
        private static VolunteerService volunteerServ;
        private static IRepository<AuthorizationModel> AuthRepo;
        /// <summary>
        /// call this once before using any of this service
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="volunteerService"></param>
        /// <param name="AuthRepository"></param>
        public static void InitService(
            TokenService tokenService,
            VolunteerService volunteerService,
            IRepository<AuthorizationModel> AuthRepository
            )
        {
            tokenServ = tokenService;
            volunteerServ = volunteerService;
            AuthRepo = AuthRepository;
        }
        /// <summary>
        /// 根据Guid找到AuthorizationModel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AuthorizationModel FindAuthorizationModel(Guid id)
        {
            return AuthRepo.FindOneById(id);
        }
        /// <summary>
        /// 根据API名称找到AuthorizationModel
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public static AuthorizationModel FindAuthorizationModel(string apiName)
        {
            AuthorizationModel authorizationModel;
            //Dictionary<string, object> queryDict = new Dictionary<string, object>();
            //queryDict.Add("ApiName", apiName);
            QueryObject<AuthorizationModel> queryObject = new QueryObject<AuthorizationModel>(AuthRepo);
            queryObject.AppendQuery(QueryOperator.Equal, "ApiName", apiName, QueryLogic.And);
            authorizationModel = AuthRepo.Find(queryObject).FirstOrDefault();
            return authorizationModel;
        }
        /// <summary>
        /// 找到所有的AuthorizationModel
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AuthorizationModel> FindAllAuthorizationModel()
        {
            return AuthRepo.FindAll();
        }

        /// <summary>
        /// 删除一个AuthorizationModel
        /// </summary>
        /// <param name="authorizationModel"></param>
        public static void DeleteOne(AuthorizationModel authorizationModel)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("_id", authorizationModel.Id);
            QueryObject<AuthorizationModel> queryObject = new QueryObject<AuthorizationModel>(AuthRepo);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            AuthRepo.Delete(queryObject);
        }

        /// <summary>
        /// 添加一个AuthorizationModel
        /// </summary>
        /// <param name="authorizationModel"></param>
        public static void InsertOne(AuthorizationModel authorizationModel)
        {
            AuthRepo.InsertOne(authorizationModel);
        }


        public static void SaveOne(AuthorizationModel authorizationModel)
        {
            AuthRepo.SaveOne(authorizationModel);
        }

        /// <summary>
        /// return is the user of this token can use this api
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ApiName"></param>
        /// <returns></returns>
        public static bool AuthorizeToken(Guid token,string ApiName)
        {
            //FIND USER
            var user=FindUserWithToken(token);
            return AuthorizeUser(user,ApiName);
        }

        public static User FindUserWithToken(Guid token)
        {
            var tm = tokenServ.ValidateToken(token);
            if (tm == null)
            {
                return User.Anonymous;
            }
            try
            {
                var user = volunteerServ.FindUser(tm.UserId);
                if (user == null)
                {
                    return User.Anonymous;
                }
                else
                {
                    return user;
                }
            }
            catch
            {
                return User.Anonymous;
            }
            
            
        }

        /// <summary>
        /// find if a user can use this api 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ApiName"></param>
        /// <returns></returns>
        public static bool AuthorizeUser(User user, string ApiName)
        {
            //find corespoding auth model
            //var dict = new Dictionary<string, object>();
            //dict.Add("ApiName", ApiName.ToLower());
            QueryObject<AuthorizationModel> queryObject = new QueryObject<AuthorizationModel>(AuthRepo);
            queryObject.AppendQuery(QueryOperator.Equal, "ApiName", ApiName.ToLower(), QueryLogic.And);
            var authModel = (AuthorizationModel)AuthRepo.Find(queryObject).FirstOrDefault();

            if (authModel == null)
                return false;

            //check is is forbid role
            foreach (var role in user.UserRole)
            {
                if (authModel.ForbidRoles.Contains(role))
                    return false;
            }

            //check if forbid user
            if (authModel.ForbidUsers.Contains(user.Id))
            {
                return false;
            }
            //check is allowed user
            if (authModel.AllowedUsers.Contains(user.Id))
            {
                return true;
            }
            //check is allowed roles
            foreach (var role in user.UserRole)
            {
                if (authModel.AllowedRoles.Contains(role))
                    return true;
            }
            //return false
            //not specified user, reject
            return false;
        }


    }
}
