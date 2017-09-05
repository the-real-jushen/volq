using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.BlogSystem
{
    public class Asset
    {
        public const string PICTURE = "picture";
        public const string VIDEO = "video";
        public const string AUDIO = "audio";
        public string Path { get; set; }
        public string Type { get; set; }
        public Asset(string path, string type)
        {
            Path = path;
            Type = type;
        }
        public Asset()
        {
            Path = "";
            Type = "";
        }

    }
}
