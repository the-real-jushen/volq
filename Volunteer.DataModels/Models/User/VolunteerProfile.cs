using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;

namespace Jtext103.Volunteer.DataModels.Models
{
    //志愿者的Profile
    public class VolunteerProfile : Profile
    {
        //该志愿者所有已经sign in的Activities的Id集合（不包括sign out的）
        public HashSet<Guid> SignedInActivityIds { get; set; }
        //该志愿者已获得的点数
        public VolunteerPoint Point { get; set; }
        //属性（六芒星）
        public HexagramProperty HexagramProperty { get; set; }
        //该志愿者等级，根据point计算
        //计算公式：pow(level - 1, 2) * 100 = totalPoint
        public int VolunteerLevel
        {
            get
            {
                return Convert.ToInt32(Math.Floor(Math.Sqrt(Point.TotalPoint / 100) + 1));
            }
        }
        //升到下一级所需点数
        public double PointsToNextLevel
        {
            get
            {
                return Math.Pow(VolunteerLevel, 2) * 100 - Point.TotalPoint;
            }
        }
        //当前等级名称
        public string VolunteerLevelName
        {
            get
            {
                switch (VolunteerLevel)
                {
                    case 1:
                        return "青铜5";
                    case 2:
                        return "青铜4";
                    case 3:
                        return "青铜3";
                    case 4:
                        return "青铜2";
                    case 5:
                        return "青铜1";
                    case 6:
                        return "白银5";
                    case 7:
                        return "白银4";
                    default:
                        return "嘴强王者";
                }
            }
        }
        //等级图标
        public string VolunteerLevelPicture
        {
            get
            {
                switch (VolunteerLevel)
                {
                    case 1:
                        return "/Static/Images/Level/1.gif";
                    case 2:
                        return "/Static/Images/Level/2.gif";
                    case 3:
                        return "/Static/Images/Level/3.gif";
                    case 4:
                        return "/Static/Images/Level/4.gif";
                    case 5:
                        return "/Static/Images/Level/5.gif";
                    default:
                        return "/Static/Images/Level/5.gif";
                }
            }
        }
        //活动地点描述
        public string Location { get; set; }
        //活动地点坐标
        public string Coordinate { get; set; }
        //指示该volunteer的从属
        public List<string> Affiliation { get; set; }
        //该志愿者的浏览记录
        public List<VolunteerViewOrFavoriteRecord> VolunteerViewActivitiesRecords { get; set; }
        //该志愿者的收藏记录
        public List<VolunteerViewOrFavoriteRecord> VolunteerFavoriteActivitiesRecords { get; set; }
        //我申请其他人好友的信息
        public List<ApplyFriendFromMe> ApplyFriendFromMe { get; set; }
        //其他人向我申请好友的信息
        public List<ApplyFriendToMe> ApplyFriendToMe { get; set; }
        public VolunteerProfile(string name)
            : base(name)
        {
            SignedInActivityIds = new HashSet<Guid>();
            Affiliation = new List<string>();
            Point = new VolunteerPoint();
            this.HexagramProperty = new HexagramProperty(0, 0, 0, 0, 0);
            VolunteerViewActivitiesRecords = new List<VolunteerViewOrFavoriteRecord>();
            VolunteerFavoriteActivitiesRecords = new List<VolunteerViewOrFavoriteRecord>();
            this.ApplyFriendFromMe = new List<ApplyFriendFromMe>();
            this.ApplyFriendToMe = new List<ApplyFriendToMe>();
            Description = "这个志愿者很懒，什么都没有留下。";
        }

        /// <summary>
        /// 完成活动获得六芒星属性
        /// </summary>
        /// <param name="hexagramProperty">活动的六芒星属性</param>
        /// <param name="point">活动的分数</param>
        public void AddActivityHexagramProperty(HexagramProperty hexagramProperty, double point)
        {
            double dampingFactor = Convert.ToDouble(Jtext103.StringConfig.ConfigString.GetString("HexagramPropertyFactor.xml", "dampingFactor"));
            HexagramProperty += hexagramProperty * (point / dampingFactor);
        }
        public static void RegisterMe(IVolunteerService volunteerService)
        {
            VolunteerPoint.RegisterMe(volunteerService);
            HexagramProperty.RegisterMe(volunteerService);
            volunteerService.RegisterMap<VolunteerProfile>();
        }
    }
    //我申请其他人好友的信息
    public class ApplyFriendFromMe
    {
        public Guid VolunteerId { get; set; }
        public string Name { get; set; }
        //申请时间
        public DateTime ApplyTime { get; set; }
        //修改状态的时间
        public DateTime ActionTime { get; set; }
        //备注（回复的信息）
        public string Comment { get; set; }
        //申请状态
        public ApplyFriendStatus Status { get; set; }
        public ApplyFriendFromMe(Guid volunteerId, string name)
        {
            VolunteerId = volunteerId;
            Name = name;
            ApplyTime = DateTime.Now;
            ActionTime = DateTime.Now;
            Status = ApplyFriendStatus.Applying;
        }
    }
    //其他人向我申请好友的信息
    public class ApplyFriendToMe
    {
        public Guid VolunteerId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        //备注（其他人填写的申请理由）
        public string Comment { get; set; }
        //是否处理
        public bool hasHandled { get; set; }
        public ApplyFriendToMe(Guid volunteerId, string name, string comment)
        {
            VolunteerId = volunteerId;
            Name = name;
            Time = DateTime.Now;
            Comment = comment;
            hasHandled = false;
        }
    }
    //申请好友的状态
    public enum ApplyFriendStatus
    {
        /// <summary>
        /// 正在申请中
        /// </summary>
        Applying,
        /// <summary>
        /// 已经接受
        /// </summary>
        Accept,
        /// <summary>
        /// 被拒绝
        /// </summary>
        Refuse,
        /// <summary>
        /// 不再是好友
        /// </summary>
        BreakOff
    }
}
