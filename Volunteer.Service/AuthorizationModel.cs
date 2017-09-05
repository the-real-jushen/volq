using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Models;


namespace Jtext103.Volunteer.Service
{
    public class AuthorizationModel
    {
        public Guid Id { get; set; }
        public string ApiName { get; set; }
        public string Description { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public List<Guid> AllowedUsers { get; set; }
        public List<Role> AllowedRoles { get; set; }

        public List<Guid> ForbidUsers { get; set; }
        public List<Role> ForbidRoles { get; set; }

        public AuthorizationModel()
        {
            AllowedUsers = new List<Guid>();
            AllowedRoles = new List<Role>();
            ForbidUsers = new List<Guid>();
            ForbidRoles = new List<Role>();
            Id = Guid.NewGuid();
            ExtraInformation = new Dictionary<string, object>();
        }
        //附加信息
        public Dictionary<string, object> ExtraInformation { get; set; }

        /// <summary>
        /// 向ExtraInformation中新加入一条信息
        /// </summary>
        /// <param name="key">格式为"writer-key"的字符串，writer为key的属于的类型，key为key的name</param>
        /// <param name="value"></param>
        public void AddExtraInformation(string key, object value)
        {
            ExtraInformation.Add(key, value);
        }

        /// <summary>
        /// 删除ExtraInformation中key对应的值
        /// </summary>
        /// <param name="key"></param>
        public bool RemoveExtraInformation(string key)
        {
            return ExtraInformation.Remove(key);
        }
    }
}