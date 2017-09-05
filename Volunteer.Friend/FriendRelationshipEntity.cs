using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.Friend
{
    public class FriendRelationshipEntity
    {
        public FriendRelationshipEntity(Guid volunteer1Id, Guid volunteer2Id)
        {
            Id = System.Guid.NewGuid();
            Volunteer1Id = volunteer1Id;
            Volunteer2Id = volunteer2Id;
        }
        public Guid Id { get; set; }
        public Guid Volunteer1Id { get; set; }
        public Guid Volunteer2Id { get; set; }
    }
}
