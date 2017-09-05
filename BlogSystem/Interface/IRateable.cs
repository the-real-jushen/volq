using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.BlogSystem.Interface
{
    public interface IRateable
    {
        //分数
        double Rating { get; }
        //打分
        void Rate(Guid userId, double rating);
    }
}
