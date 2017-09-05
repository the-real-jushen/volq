using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Repository.Interface;
using Jtext103.Repository;

namespace Jtext103.Volunteer.VolunteerMessage
{
    public class MessageService
    {
        private IRepository<Message> messageRepository;
        public MessageService(IRepository<Message> messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        //获得所有发给该用户的message
        public IEnumerable<Message> FindMyMessages(Guid receiverId, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            QueryObject<Message> queryObject = new QueryObject<Message>(messageRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("ReceiverId", receiverId);
            queryDic.Add("HasDeleted", false);
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            List<Message> myMessages = new List<Message>();
            myMessages = messageRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize).ToList();
            return myMessages;
        }

        //获得发给该用户的已读或未读message
        public IEnumerable<Message> FindMyReadOrNotReadMessage(Guid receiverId, string messageFrom, bool hasRead, string sortByKey, bool isAscending, int pageIndex, int pageSize)
        {
            QueryObject<Message> queryObject = new QueryObject<Message>(messageRepository);
            Dictionary<string, object> queryDic = new Dictionary<string, object>();
            queryDic.Add("ReceiverId", receiverId);
            if (messageFrom != null && messageFrom != "")
            {
                queryDic.Add("MessageFrom", messageFrom);
            }
            queryDic.Add("HasDeleted", false);
            if (hasRead == true)
            {
                queryDic.Add("HasRead", true);
            }
            else
            {
                queryDic.Add("HasRead", false);
            }
            queryObject.AppendQuery(queryDic, QueryLogic.And);
            List<Message> myMessages = new List<Message>();
            myMessages = messageRepository.Find(queryObject, sortByKey, isAscending, pageIndex, pageSize).ToList();
            return myMessages;
        }

        /// <summary>
        /// 发送一条message，并存数据库
        /// </summary>
        public void SendMessage(string messageFrom, Guid receiverId, string title, string text, List<string> pictures, string destinationLink, bool newBlank)
        {
            Message newMessage = new Message(messageFrom, receiverId, title, text, pictures, destinationLink, newBlank);
            messageRepository.SaveOne(newMessage);
        }


        /// <summary>
        /// 将message标记为已读
        /// 当readUserId==message.ReceiverId时，才会标记为已读
        /// </summary>
        /// <param name="messageId">该message的id</param>
        /// <param name="readUserId">读该message的userid</param>
        public void ReadMessage(Guid messageId, Guid readUserId)
        {
            Message message = messageRepository.FindOneById(messageId);
            if (message.ReceiverId == readUserId)
            {
                if (message.HasRead == false)
                {
                    message.HasRead = true;
                    messageRepository.SaveOne(message);
                }
            }
        }

        /// <summary>
        /// 将message标记为已删除
        /// </summary>
        /// <param name="message"></param>
        public void DeleteMessage(Guid messageId)
        {
            Message message = messageRepository.FindOneById(messageId);
            if (message.HasDeleted == false)
            {
                message.HasDeleted = true;
                messageRepository.SaveOne(message);
            }
        }

    }
}
