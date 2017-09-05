using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.Badge
{
    public class BadgeEntity
    {
        public BadgeEntity(Guid userId, string badgeName)
        {
            Id = System.Guid.NewGuid();
            UserId = userId;
            BadgeName = badgeName;
            WetherRequirementSatisfaction = new Dictionary<string,bool>();
            IsGranted = false;
        }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BadgeName { get; set; }
        public Dictionary<string, bool> WetherRequirementSatisfaction { get; set; }
        /// <summary>
        /// is the badge has been granted to the user
        /// </summary>
        public bool IsGranted { get; set; }
        //获得该badge的时间
        public DateTime GrantedTime { get; set; }
    }
}
