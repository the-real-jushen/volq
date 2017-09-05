using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.BlogSystem
{
    public class CommentEntity : BlogEntity
    {
        //父comment id
        //没有父comment时，为Guid.Empty
        public Guid FatherId { get; set; }
        //子comment id
        public List<Guid> ChildrenId { get; set; }
        //该评论位于的楼层（从1开始）
        public int Position { get; set; }

        public CommentEntity(BasicUser user, Guid targetId, Guid fatherId, string content)
            : base(user, targetId, content)
        {
            FatherId = fatherId;
            ChildrenId = new List<Guid>();
            Position = 0;
        }
    }
}
