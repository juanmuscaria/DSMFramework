using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace ModLoader.Modding
{
    public class XmlHelper
    {
        public static string ObjectToString(object toSerialize)
        {
            var xml = "";
            using(var sww = new StringWriter())
            {
                using(XmlWriter writer = XmlWriter.Create(sww))
                {
                    if (toSerialize is IDictionary dictionary)
                    {
                        Debug.Log("aaaaaaaaaa");
                        List<Entry> entries = new List<Entry>(dictionary.Count);
                        foreach (object key in dictionary.Keys)
                        {
                            entries.Add(new Entry(key, dictionary[key]));
                        }
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
                        serializer.Serialize(writer, entries);
                    }
                    else
                    {
                        Debug.Log("bbbbbbbbbbbbb");
                        XmlSerializer serializer = new XmlSerializer(toSerialize.GetType());
                        serializer.Serialize(writer, toSerialize);
                    }
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        public static T StringToObject<T>(string xml) where T : new()
        {
            if (typeof(T).IsAssignableFrom(typeof(IDictionary)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
                List<Entry> list = (List<Entry>) serializer.Deserialize(GenerateStreamFromString(xml));
                IDictionary dictionary = (IDictionary) new T();
                foreach (Entry entry in list)
                {
                    dictionary[entry.Key] = entry.Value;
                }

                return (T) dictionary;
            }
            else {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(GenerateStreamFromString(xml));
            }
        }
        
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static T FileToObject<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath)) return default;
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);  
                string str = sr.ReadLine();
                StringBuilder builder = new StringBuilder();
                while (str != null)
                {
                    builder.Append(str);
                    str = sr.ReadLine();  
                }  
                Console.ReadLine(); 
                sr.Close();  
                fs.Close();
                return StringToObject<T>(builder.ToString());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return default;
        }

        public static void ObjectToFile(object toSerialize, string filePath)
        {
            File.WriteAllText(filePath,ObjectToString(toSerialize));
        }
        
        public class Entry
        {
            public object Key;
            public object Value;
            public Entry()
            {
            }

            public Entry(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}