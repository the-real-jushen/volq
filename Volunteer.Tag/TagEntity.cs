using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.Tag
{
    public class TagEntity
    {
        public TagEntity(string name)
        {
            Id = System.Guid.NewGuid();
            Name = name;
            Alias = new List<string>();
            Alias.Add(name);
            Frequency = 1;
            Point = 1;
        }
        public Guid Id { get; set; }
        //tag的名字
        public string Name { get; set; }
        //tag的使用频率
        public int Frequency { get; set; }
        //tag的点数，用于热度排名
        public int Point { get; set; }
        //tag的别名
        public List<string> Alias { get; set; }
    }
}
