using Jtext103.Volunteer.DataModels.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class VolunteerViewOrFavoriteRecord
    {
        public Guid ActivityId { get; set; }
        public bool IsViewOrFavorite { get; set; }
        public DateTime WhenViewOrFavorite { get; set; }
        public VolunteerViewOrFavoriteRecord(Guid activityId)
        {
            WhenViewOrFavorite = new DateTime();
            ActivityId = activityId;
        }
    }
}
