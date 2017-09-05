using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using Jtext103.Volunteer.DataModels.Interface;
namespace Jtext103.Volunteer.DataModels.Models
{
    [BsonKnownTypes(typeof(VolunteerProfile), typeof(OrganizerProfile), typeof(OrganizationProfile))]
    public class Profile
    {
        public string ProfileName { get; set; }

        public UserAvatar Avatar { get; set; }

        public string Description { get; set; }

        public Profile(string name)
        {
            ProfileName = name;
            Avatar = new UserAvatar();
        }
    }
}
