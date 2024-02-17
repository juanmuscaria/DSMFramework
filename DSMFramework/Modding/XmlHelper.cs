using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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
            using var sww = new StringWriter();
            using var writer = XmlWriter.Create(sww);
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

            return sww.ToString();
        }

        /// <summary>
        ///     Loads a object from a file
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <typeparam name="T">The type of the object to be restored</typeparam>
        /// <returns>The restored object</returns>
        public static T FileToObject<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return default;
                }

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                if (typeof(IDictionary).IsAssignableFrom(typeof(T)))
                {
                    var serializer = new XmlSerializer(typeof(List<Entry>));
                    var list = (List<Entry>)serializer.Deserialize(sr);
                    var dictionary = (IDictionary)new T();
                    foreach (var entry in list) dictionary[entry.Key] = entry.Value;

                    return (T)dictionary;
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(sr);
                }


            }
            catch (Exception e)
            {
                Plugin.LOGGER.LogError("Unable to load object from xml");
                Plugin.LOGGER.LogError(e);
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