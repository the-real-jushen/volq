using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.Badge
{
    public class BadgeDescription
    {
        public BadgeDescription(string badgeName, string description, Dictionary<string, string> requirementDescription)
        {
            Id = System.Guid.NewGuid();
            BadgeName = badgeName;
            Description = description;
            RequirementDescription = requirementDescription;
        }
        public Guid Id { get; set; }
        public string BadgeName { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
        public Dictionary<string, string> RequirementDescription { get; set; }

    }
}
