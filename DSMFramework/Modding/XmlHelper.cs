using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace DSMFramework.Modding
{
    /// <summary>
    ///     A helper class to do all hard work of dealing with xml serialization
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        ///     Converts the given object into a xml string if possible
        /// </summary>
        /// <param name="toSerialize">An object to serialize</param>
        /// <returns>A string representation of that object</returns>
        public static string ObjectToString(object toSerialize)
        {
            string xml;
            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    if (toSerialize is IDictionary dictionary)
                    {
                        var entries = new List<Entry>(dictionary.Count);
                        foreach (var key in dictionary.Keys) entries.Add(new Entry(key, dictionary[key]));
                        var serializer = new XmlSerializer(typeof(List<Entry>));
                        serializer.Serialize(writer, entries);
                    }
                    else
                    {
                        var serializer = new XmlSerializer(toSerialize.GetType());
                        serializer.Serialize(writer, toSerialize);
                    }

                    xml = sww.ToString();
                }
            }

            return xml;
        }

        /// <summary>
        ///     Converts a xml string into an object
        /// </summary>
        /// <param name="xml">A xml string to be converted</param>
        /// <typeparam name="T">The type of the object to be restored</typeparam>
        /// <returns>The restored object</returns>
        public static T StringToObject<T>(string xml) where T : new()
        {
            if (typeof(T).IsAssignableFrom(typeof(IDictionary)))
            {
                var serializer = new XmlSerializer(typeof(List<Entry>));
                var list = (List<Entry>) serializer.Deserialize(GenerateStreamFromString(xml));
                var dictionary = (IDictionary) new T();
                foreach (var entry in list) dictionary[entry.Key] = entry.Value;

                return (T) dictionary;
            }
            else
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(GenerateStreamFromString(xml));
            }
        }

        /// <summary>
        ///     Converts a string into a stream
        /// </summary>
        /// <param name="toConvert">An string to be converted into a stream</param>
        /// <returns>The stream of the string</returns>
        public static Stream GenerateStreamFromString(string toConvert)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(toConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        ///     Loads a object from a file
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <typeparam name="T">The type of the object to be restored</typeparam>
        /// <returns>The restored object</returns>
        public static T FileToObject<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath)) return default;
            try
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                var str = sr.ReadLine();
                var builder = new StringBuilder();
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

        /// <summary>
        ///     Save an object into a file
        /// </summary>
        /// <param name="toSerialize">The object to save</param>
        /// <param name="filePath">The file path to save the object</param>
        public static void ObjectToFile(object toSerialize, string filePath)
        {
            File.WriteAllText(filePath, ObjectToString(toSerialize));
        }
        
        public class Entry
        {
            public readonly object Key;
            public readonly object Value;

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