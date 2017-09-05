using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.Volunteer.DataModels.Models
{
    public class Camparer<T> : IComparer<T>
    {
        //进行比较对象的类型
        private Type type = null;
        //进行比较对象的属性名称
        private string name;
        //是否是递增排序（true,递增;false,递减）
        private bool ascending;

        public Camparer(Type type, string name, bool isAscending)
        {
            this.type = type;
            this.name = name;
            this.ascending = isAscending;
        }

        //实现IComparer<T>的比较方法
        int IComparer<T>.Compare(T x, T y)
        {
            object object1 = this.type.InvokeMember(this.name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, x, null);
            object object2 = this.type.InvokeMember(this.name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, y, null);
            //PropertyInfo property = this.type.GetProperty(this.name);
            //object object1 = property.GetValue(x);
            //object object2 = property.GetValue(y);
            //递增排序
            if (ascending == true)
            {
                return (new CaseInsensitiveComparer()).Compare(object1, object2);
            }
            //递减排序
            else
            {
                return (new CaseInsensitiveComparer()).Compare(object2, object1);
            }
        }
    }
}
