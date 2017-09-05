 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.VolunteerMessage
{
    public class Message
    {
        public Message(string messageFrom, Guid receiverId, string title, string text, List<string> pictures, string destinationLink, bool newBlank)
        {
            Id = System.Guid.NewGuid();
            MessageFrom = messageFrom;
            ReceiverId = receiverId;
            Title = title;
            Text = text;
            Time = DateTime.Now;
            Pictures = pictures;
            DestinationLink = destinationLink;
            NewBlank = newBlank;
            HasRead = false;
            HasDeleted = false;
        }
        public Guid Id { get; set; }
        //标题
        public string Title { get; set; }
        //内容
        public string Text { get; set; }
        //（feed用的）图片
        public List<string> Pictures { get; set; }
        //（feed用的）点击时跳转的链接
        public string DestinationLink { get; set; }
        //（feed用的）是否在新标签中打开
        public bool NewBlank { get; set; }
        //发送的时间
        public DateTime Time { get; set; }
        //接收用户的id
        public Guid ReceiverId { get; set; }
        //发送者的id（如果是系统等无id发送者，则为描述字符串）
        public string MessageFrom { get; set; }
        //标记接收用户是否读过
        public bool HasRead { get; set; }
        //标记用户是否删除
        public bool HasDeleted { get; set; }
    }
}
