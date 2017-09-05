using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;

namespace Jtext103.StringConfig
{
    public class ConfigString
    {
        static Dictionary<string, Dictionary<string, string>> strings;

        /// <summary>
        /// returns a string value corresponding string to the given key; 
        /// this will not throw a exception, but return a empty string if not key is found
        /// </summary>
        /// <param name="fileName">xmlFileName(include extension)</param>
        /// <param name="key">the key to the string</param>
        /// <returns></returns>
        public static string GetString(string xmlFileName, string key)
        {
            string lowerXmlFileName = xmlFileName.ToLower();
            if (strings.ContainsKey(lowerXmlFileName))
            {
                if (strings[lowerXmlFileName].ContainsKey(key))
                {
                    return strings[lowerXmlFileName][key];
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// returns a string value corresponding string to the given key; 
        /// this will throw a exception if not key is found
        /// </summary>
        /// <param name="xmlFileName">xmlFileName(include extension)</param>
        /// <param name="key">the key to the string</param>
        /// <returns></returns>
        public static string TryGetString(string xmlFileName, string key)
        {
            string lowerXmlFileName = xmlFileName.ToLower();
            if (strings.ContainsKey(lowerXmlFileName))
            {
                if (strings[lowerXmlFileName].ContainsKey(key))
                {
                    return strings[lowerXmlFileName][key];
                }
            }
            throw new System.Exception("no key found");
        }
        /*
        /// <summary>
        /// put a string in the strings configration
        /// </summary>
        /// <param name="xmlFileName">xmlFileName(include extension)</param>
        /// <param name="key">the key</param>
        /// <param name="value">the string value you need to store</param>
        public static void PutString(string xmlFileName, string key, string value)
        {
            if (strings.ContainsKey(xmlFileName))
            {
                if (strings[xmlFileName].ContainsKey(key))
                {
                    strings[xmlFileName][key] = value;
                }
            }
            else
            {
                strings[xmlFileName].Add(key, value);
            }
        }
        */
        /*
        /// <summary>
        /// save the string into a config file in xml format
        /// </summary>
        /// <param name="path">file path</param>
        public static void Save(string path)
        {
            DataContractSerializer Xs = new DataContractSerializer(typeof(Dictionary<string, string>));
            //Stream Fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            
            var settings = new XmlWriterSettings { Indent = true };
            var fs = XmlWriter.Create(path, settings);
            
            Xs.WriteObject(fs, strings);
            fs.Close();
        }
        */
        public static void Load(string folderPath)
        {
            //DataContractSerializer Xs = new DataContractSerializer(typeof(Dictionary<string, string>));
            //Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //strings = (Dictionary<string, string>)Xs.ReadObject(fs);
            //fs.Close();
            strings = new Dictionary<string, Dictionary<string, string>>();
            DirectoryInfo folder = new DirectoryInfo(folderPath);
            foreach (FileInfo file in folder.GetFiles())
            {
                DataContractSerializer xs = new DataContractSerializer(typeof(Dictionary<string, string>));
                Stream fs = new FileStream(folderPath + "\\" + file.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Dictionary<string, string> content = (Dictionary<string, string>)xs.ReadObject(fs);
                strings.Add(file.Name.ToLower(), content);
                fs.Close();
            }
        }

        /// <summary>
        /// if you do not load from a file, do this before any uses
        /// </summary>
        public static void Init()
        {
            strings = new Dictionary<string, Dictionary<string, string>>();
        }
    }
}
