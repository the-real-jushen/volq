using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.StringConfig
{
    public class GenerateStringFromTemplate
    {
        /// <summary>
        /// 将template中所有在!##!内标记的部分（包括!##!）替换为对应字典中对应的值
        /// 如：字典中包含<aaa, bbb>，则将template中所有!#aaa#!替换为bbb
        /// </summary>
        /// <param name="template"></param>
        /// <param name="replaceDic"></param>
        /// <returns></returns>
        public static string GenerateString(string template, Dictionary<string, string> replaceDic)
        {
            string result = null;
            foreach (string key in replaceDic.Keys)
            {
                string replaceString = "!#" + key + "#!";
                result = template.Replace(replaceString, replaceDic[key]);
            }
            return result;
        }
    }
}
