using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Repository;

namespace Jtext103.BlogSystem
{
    public class BlogService
    {
        private IRepository<CommentEntity> commentRepository;
        private IRepository<BlogPostEntity> blogPostRepository;
        public BlogService(IRepository<CommentEntity> commentRepository, IRepository<BlogPostEntity> blogPostRepository)
        {
            this.commentRepository = commentRepository;
            this.blogPostRepository = blogPostRepository;
            //注册BlogPost的私有属性
            blogPostRepository.RegisterMap<BlogPostEntity>(new List<string>() { "readTimes", "likedTimes", "reprintedTimes", "rating" });
        }

        public void AddComment(BasicUser user, Guid targetId, Guid fatherId, string content)
        {
            //先计算楼层
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            QueryObject<CommentEntity> queryObject = new QueryObject<CommentEntity>(commentRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            int position = Convert.ToInt32(commentRepository.FindCountOfResult(queryObject)) + 1;
            //加入数据库
            CommentEntity newComment = new CommentEntity(user, targetId, fatherId, content);
            newComment.Position = position;
            commentRepository.SaveOne(newComment);
            //当存在father时
            //在father的children中加入该comment
            if (fatherId != Guid.Empty)
            {
                CommentEntity father = FindComment(fatherId);
                father.ChildrenId.Add(newComment.Id);
                commentRepository.SaveOne(father);
            }
        }

        public void AddBlogPost(BasicUser user, Guid targetId, string title, string content)
        {
            BlogPostEntity blogPostEntity = new BlogPostEntity(user, targetId, title, content);
            blogPostRepository.SaveOne(blogPostEntity);
        }

        /// <summary>
        /// 将comment标记为已删除
        /// </summary>
        public void DeleteComment(Guid commentId)
        {
            CommentEntity commentEntity = FindComment(commentId);
            commentEntity.HasDeleted = true;
            commentRepository.SaveOne(commentEntity);
        }

        /// <summary>
        /// 将blogPost标记为已删除
        /// </summary>
        public void DeleteBlogPost(Guid blogPostId)
        {
            BlogPostEntity blogPostEntity = FindBlogPost(blogPostId);
            blogPostEntity.HasDeleted = true;
            blogPostRepository.SaveOne(blogPostEntity);
        }

        /// <summary>
        /// 生成该comment所显示的内容
        /// </summary>
        public string GenerateDisplayContent(CommentEntity commentEntity)
        {
            if (commentEntity.HasDeleted == true)
            {
                return "该评论已被删除！";
            }
            //如果没有father，直接返回该comment的content
            if (commentEntity.FatherId == Guid.Empty)
            {
                return commentEntity.Content;
            }
            //如果有father，需要在content中显示father的名字
            else
            {
                string fatherName = FindComment(commentEntity.FatherId).User.UserName;
                string content = "【回复 " + fatherName + "】" + commentEntity.Content;
                return content;
            }
        }

        /// <summary>
        /// 激活blogPost
        /// </summary>
        public void ActivateBlogPost(BlogPostEntity blogPostEntity)
        {
            if (blogPostEntity.IsActivated == false)
            {
                blogPostEntity.IsActivated = true;
                blogPostEntity.ModifyTime = DateTime.Now;
                blogPostRepository.SaveOne(blogPostEntity);
            }
        }

        /// <summary>
        /// 修改blogPost
        /// </summary>
        public void ModifyBlogPost(BlogPostEntity blogPostEntity, string title, string content)
        {
            blogPostEntity.Title = title;
            blogPostEntity.Content = content;
            blogPostEntity.ModifyTime = DateTime.Now;
            blogPostRepository.SaveOne(blogPostEntity);
        }

        #region find comment
        public CommentEntity FindComment(Guid commentId)
        {
            CommentEntity result = commentRepository.FindOneById(commentId);
            return result;
        }

        /// <summary>
        /// 所有对targetId的评论的个数
        /// </summary>
        public long FindAllCommentsCount(Guid targetId)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            QueryObject<CommentEntity> queryObject = new QueryObject<CommentEntity>(commentRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            long result = commentRepository.FindCountOfResult(queryObject);
            return result;
        }

        /// <summary>
        /// 找到所有对targetId的评论
        /// </summary>
        public IEnumerable<CommentEntity> FindAllComments(Guid targetId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            QueryObject<CommentEntity> queryObject = new QueryObject<CommentEntity>(commentRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<CommentEntity> result = commentRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// 找到所有对targetId的根评论
        /// </summary>
        public IEnumerable<CommentEntity> FindAllRootComments(Guid targetId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            queryDict.Add("FatherId", Guid.Empty);
            QueryObject<CommentEntity> queryObject = new QueryObject<CommentEntity>(commentRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<CommentEntity> result = commentRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            return result;
        }
        #endregion find comment

        #region find blogPost
        public BlogPostEntity FindBlogPost(Guid blogPostId)
        {
            BlogPostEntity result = blogPostRepository.FindOneById(blogPostId);
            return result;
        }

        /// <summary>
        /// 找到所有对targetId的BlogPost的个数
        /// </summary>
        public long FindAllBlogPostCount(Guid targetId)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            QueryObject<BlogPostEntity> queryObject = new QueryObject<BlogPostEntity>(blogPostRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            long result = blogPostRepository.FindCountOfResult(queryObject);
            return result;
        }

        /// <summary>
        /// 找到所有对targetId的BlogPost
        /// </summary>
        public IEnumerable<BlogPostEntity> FindAllBlogPost(Guid targetId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("TargetId", targetId);
            QueryObject<BlogPostEntity> queryObject = new QueryObject<BlogPostEntity>(blogPostRepository);
            queryObject.AppendQuery(queryDict, QueryLogic.And);
            IEnumerable<BlogPostEntity> result = blogPostRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize);
            return result;
        }

        #endregion find blogPost
    }
}
