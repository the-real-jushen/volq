using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;

namespace Jtext103.Volunteer.DataModels.Models
{
    //组织的Profile
    public class OrganizationProfile : Profile
    {
        //属于该Organize的所有Activity Id
        public HashSet<Guid> ActivityIds { get; set; }
        //属于该Organize的所有Organizer Id
        public HashSet<Guid> OrganizerIds { get; set; }
        //申请加入该Organize的所有Organizer的申请信息
        public List<ApplyOrganizerInformationByOrganization> ApplyOrganizerInformation { get; set; }
        //该组织每个月可用总分
        public double TotalPointEachMonth { get; set; }
        //该组织该月总分余额
        public double RemainingSum { get; set; }
        //消耗的总分数
        public double ConsumeAllPoint { get; set; }
        //总活动数（在更新statistic的时候更新）
        public int AllActivityCount { get; set; }
        //总参加人次（在更新statistic的时候更新）
        public int AllVolunteerCount { get; set; }
        //统计信息
        public List<OrganizationStatistic> OrganizationStatistics { get; set; }
        public OrganizationProfile(string name, double totalPointEachMonth)
            : base(name)
        {
            ActivityIds = new HashSet<Guid>();
            OrganizerIds = new HashSet<Guid>();
            ApplyOrganizerInformation = new List<ApplyOrganizerInformationByOrganization>();
            OrganizationStatistics = new List<OrganizationStatistic>();
            TotalPointEachMonth = totalPointEachMonth;
            RemainingSum = totalPointEachMonth;
            ConsumeAllPoint = 0;
            Description = "这个组织很懒，什么都没有留下。";
        }
        public void UpdateStatistics(User Organization, IVolunteerService myService)
        {
            //检查是否需要新加统计信息
            foreach (OrganizationStatistic existStatistic in OrganizationStatistics)
            {
                //上个月的统计信息已存
                if ((existStatistic.Date.Year == DateTime.Now.Year) && (existStatistic.Date.Month == DateTime.Now.Month - 1))
                {
                    return;
                }
                //当过了一年，需要检查去年12月统计信息
                else if ((existStatistic.Date.Year == DateTime.Now.Year - 1) && (existStatistic.Date.Month == 12) && (DateTime.Now.Month == 1))
                {
                    return;
                }
            }
            //新建上个月的统计
            OrganizationStatistic newStatistic = new OrganizationStatistic();
            int year, month;
            if (DateTime.Now.Month == 1)
            {
                year = DateTime.Now.Year - 1;
                month = 12;
            }
            else
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month - 1;
            }
            foreach (Activity activity in myService.FindAllNotDraftActivities(Organization, "", "", true, 0, 0))
            {
                if ((activity.ActivateTime.Year == year) && (activity.ActivateTime.Month == month))
                {
                    newStatistic.ConsumePoint += activity.Point;
                    newStatistic.ActivityCount++;
                    newStatistic.HexagramProperty += activity.HexagramProperty;
                    newStatistic.VolunteerCount += activity.VolunteerStatus.Count;
                }
            }
            newStatistic.RemainPoint = TotalPointEachMonth - newStatistic.ConsumePoint;
            OrganizationStatistics.Add(newStatistic);
            //更新总活动数和总参加人次
            AllActivityCount += newStatistic.ActivityCount;
            AllVolunteerCount += newStatistic.VolunteerCount;
            myService.SaveOne(Organization);
        }
        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<OrganizationProfile>();
        }
    }
    public class ApplyOrganizerInformationByOrganization
    {
        //organizer的id
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; }
        public DateTime Time { get; set; }
        //备注（由Organizer填写申请理由）
        public string Comment { get; set; }
        //是否处理
        public bool hasHandled { get; set; }
        public ApplyOrganizerInformationByOrganization(Guid organizerId, string organizerName, string comment)
        {
            OrganizerId = organizerId;
            OrganizerName = organizerName;
            Time = DateTime.Now;
            Comment = comment;
            hasHandled = false;
        }
    }

    /// <summary>
    /// organization每月的统计
    /// </summary>
    public class OrganizationStatistic
    {
        //时间（每个月创建一个新的statistic）
        public DateTime Date { get; set; }
        //该月分数总消耗
        public double ConsumePoint { get; set; }
        //该月分数余额
        public double RemainPoint { get; set; }
        //本月活动数
        public int ActivityCount { get; set; }
        //本月参加人次
        public int VolunteerCount { get; set; }
        //六芒星属性
        public HexagramProperty HexagramProperty { get; set; }
        public OrganizationStatistic()
        {
            Date = DateTime.Now;
            ConsumePoint = 0;
            RemainPoint = 0;
            ActivityCount = 0;
            VolunteerCount = 0;
            HexagramProperty = new HexagramProperty(0, 0, 0, 0, 0);
        }
    }
}
