using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class TokenModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime DueTime { get; set; }
        public TokenModel(Guid userId, DateTime dueTime)
        {
            Id = System.Guid.NewGuid();
            UserId = userId;
            DueTime = dueTime;
        }
    }
}
