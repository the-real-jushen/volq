using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.BlogSystem.Interface;

namespace Jtext103.BlogSystem
{
    public class BlogPostEntity : BlogEntity, IRateable, ILikeable
    {
        //标题
        public string Title { get; set; }
        //是否已激活
        public bool IsActivated { get; set; }
        //修改时间
        public DateTime ModifyTime { get; set; }
        public Asset Asset { get; set; }
        //阅读次数
        public int ReadTimes
        {
            get
            {
                return readTimes;
            }
        }
        //点赞次数
        public int LikedTimes
        {
            get
            {
                return likedTimes;
            }
        }
        //转载次数
        public int ReprientedTimes
        {
            get
            {
                return reprintedTimes;
            }
        }
        //得分
        public double Rating
        {
            get
            {
                return rating;
            }
        }

        private int readTimes;
        private int likedTimes;
        private int reprintedTimes;
        private int rating;
        public BlogPostEntity(BasicUser user, Guid targetId, string title, string content)
            : base(user, targetId, content)
        {
            Title = title;
            IsActivated = false;
            ModifyTime = DateTime.Now;
            this.Asset = new Asset();
            readTimes = 0;
            likedTimes = 0;
            reprintedTimes = 0;
            rating = 0;
        }
        public void Rate(Guid userId, double rating)
        {
            throw new NotImplementedException();
        }

        public void Like(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void UnLike(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
