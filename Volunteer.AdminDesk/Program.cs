using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Volunteer.AdminDesk
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class WebApi:IList<string>
    {
        public List<string> all = new List<string> { "/api/user","/api/organizer"};

        public int IndexOf(string item)
        {
            return all.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            all.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            all.RemoveAt(index);
        }

        public string this[int index]
        {
            get
            {
                return all[index];
            }
            set
            {
                all[index] = value;
            }
        }

        public void Add(string item)
        {
            all.Add(item);
        }

        public void Clear()
        {
            all.Clear();
        }

        public bool Contains(string item)
        {
            return all.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            all.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(string item)
        {
            return all.Remove(item);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return all.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return all.GetEnumerator();
        }
    }
}
