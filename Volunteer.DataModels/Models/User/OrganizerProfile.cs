using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Volunteer.DataModels.Interface;

namespace Jtext103.Volunteer.DataModels.Models
{
    //管理员的Profile
    public class OrganizerProfile : Profile
    {
        //该organizer隶属的organization(组织id和加入时间)
        public Dictionary<Guid, DateTime> MyOrganizations { get; set; }
        //申请组织的历史记录
        public List<ApplyOrganizationInformationByOrganizer> ApplyOrganizationInformation { get; set; }
        public OrganizerProfile(string name)
            : base(name)
        {
            MyOrganizations = new Dictionary<Guid, DateTime>();
            ApplyOrganizationInformation = new List<ApplyOrganizationInformationByOrganizer>();
            Description = "这个组织者很懒，什么都没有留下。";
        }
        public static void RegisterMe(IVolunteerService volunteerService)
        {
            volunteerService.RegisterMap<OrganizerProfile>();
        }
    }
    public class ApplyOrganizationInformationByOrganizer
    {
        //申请组织的id
        public Guid ApplyOrganizationId { get; set; }
        //申请组织的名称
        public string ApplyOrganizationName { get; set; }
        //申请时间
        public DateTime ApplyTime { get; set; }
        //修改状态的时间
        public DateTime ActionTime { get; set; }
        //申请状态
        public ApplyOrganizationStatus Status { get; set; }
        //备注（由Organization填写申请意见）
        public string Comment { get; set; }
        public ApplyOrganizationInformationByOrganizer(Guid applyOrganizationId, string applyOrganizationName)
        {
            ApplyOrganizationId = applyOrganizationId;
            ApplyOrganizationName = applyOrganizationName;
            ApplyTime = DateTime.Now;
            ActionTime = DateTime.Now;
            Status = ApplyOrganizationStatus.Applying;
        }
    }
    public enum ApplyOrganizationStatus
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
        /// 被从组织中踢出
        /// </summary>
        KickOut,
        /// <summary>
        /// 主动退出组织
        /// </summary>
        Quit
    }
}
