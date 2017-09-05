using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;

namespace Jtext103.Volunteer.DataModels.Models
{
    /// <summary>
    /// the father of User and Activity
    /// </summary>
    public class Entity
    {
        protected static IVolunteerService _serviceContext;

        public Entity()
        {
            Id = System.Guid.NewGuid();
            ExtraInformation = new Dictionary<string, object>();
        }
        public Guid Id { get; set; }

        public string EntityType { get; set; }

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
            this.Save();
        }

        /// <summary>
        /// 修改ExtraInformation中指定key的内容
        /// 如果该key不存在，则向ExtraInformation中新加入新信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ModifyExtraInformation(string key, object value)
        {
            if (ExtraInformation.ContainsKey(key))
            {
                ExtraInformation[key] = value;
            }
            else
            {
                ExtraInformation.Add(key, value);
            }
            this.Save();
        }

        /// <summary>
        /// 删除ExtraInformation中key的类型为keysWriter的所有值
        /// </summary>
        /// <param name="keysWriter"></param>
        public void RemoveExtraInformation(string keysWriter)
        {
            IEnumerable<string> keys = ExtraInformation.Keys;
            foreach (string key in keys)
            {
                if (key.IndexOf(keysWriter) == 0)
                {
                    ExtraInformation.Remove(key);
                }
            }
            this.Save();
        }

        internal IVolunteerService ServiceContext
        {
            get { return _serviceContext; }
        }
        public static void SetServiceContext(IVolunteerService volunteerService)
        {
            _serviceContext = volunteerService;
        }
        public void Save()
        {
            _serviceContext.SaveOne(this);
        }
    }
}
