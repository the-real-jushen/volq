using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Repository;

namespace Jtext103.Volunteer.Tag
{
    public class TagService
    {
        private IRepository<TagEntity> tagRepository;
        public TagService(IRepository<TagEntity> tagRepository)
        {
            this.tagRepository = tagRepository;
        }

        /// <summary>
        /// 从tag pool中找到所有的tag的name
        /// </summary>
        /// <returns></returns>
        public List<string> FindAllTagsName()
        {
            List<string> result=new List<string>();
            IEnumerable<TagEntity> allTags = tagRepository.FindAll();
            foreach(TagEntity tag in allTags)
            {
                result.Add(tag.Name);
            }
            return result;
        }

        /// <summary>
        /// 找到给定name的tag(包括别名)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TagEntity FindTag(string name)
        {
            QueryObject<TagEntity> queryObject = new QueryObject<TagEntity>(tagRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("Alias", name);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            IEnumerable<TagEntity> result = tagRepository.Find(queryObject);
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
                throw new Exception("相同名字的tag有且只有一个！");
            }
        }

        /// <summary>
        /// 向tag pool中加入一个新的tag
        /// 如果已存在这个tag，则将这个tag的使用频率+1
        /// </summary>
        /// <param name="name"></param>
        public void AddTag(string name)
        {
            TagEntity existTag = FindTag(name);
            //在tag pool中找不到这个name时才能新建这个name的tag
            if (existTag == null)
            {
                TagEntity newTag = new TagEntity(name);
                tagRepository.SaveOne(newTag);
            }
            //如果在tag pool中已经找到，则将这个tag的使用频率+1
            else
            {
                IncreaseFrequency(existTag);
            }
        }
        public void AddTag(IEnumerable<string> tags)
        {
            foreach(string tag in tags)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// 该tag的使用频率+1，并将point设为该频率
        /// </summary>
        /// <param name="tag"></param>
        public void IncreaseFrequency(TagEntity tag)
        {
            tag.Frequency++;
            tag.Point = tag.Frequency;
            tagRepository.SaveOne(tag);
        }

        /// <summary>
        /// 找到最热的tag的name
        /// </summary>
        /// <param name="number">需要tag的数量（如果总数比number小，则返回所有）</param>
        /// <returns></returns>
        public List<string> FindHotTags(int number)
        {
            //tag pool中所有tag的数目
            long allTagsCount = tagRepository.FindAllCount();
            //如果需要tag数比所有tag都多，则返回所有tag
            if (allTagsCount <= number)
            {
                IEnumerable<TagEntity> allTags = tagRepository.FindAll("Point", false);
                List<string> result = new List<string>();
                foreach (TagEntity tag in allTags)
                {
                    result.Add(tag.Name);
                }
                return result;                
            }
            //否则返回number数量的tag
            else
            {
                IEnumerable<TagEntity> tags = tagRepository.FindAll("Point", false, 1, number);
                List<string> result = new List<string>();
                foreach (TagEntity tag in tags)
                {
                    result.Add(tag.Name);
                }
                return result;     
            }
        }
    }
}
