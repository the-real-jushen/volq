using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Identity.Interfaces;
using Jtext103.Identity.Models;

namespace Jtext103.BlogSystem
{
    public class BasicUser : IUser
    {
        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
        public HashSet<string> Roles { get; set; }
        public SecurityStamp Stamp { get; set; }
        public string UserName { get; set; }

        public BasicUser(Guid id, string name)
        {
            Id = id;
            UserName = name;
            Roles = new HashSet<string>();
            Stamp = new SecurityStamp();
        }
    }
}
