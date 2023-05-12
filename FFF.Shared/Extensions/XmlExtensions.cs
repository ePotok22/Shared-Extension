using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FFF.Shared
{
    public static class XmlExtensions
    {
        private static readonly Dictionary<RuntimeTypeHandle, XmlSerializer> ms_serializers = new Dictionary<RuntimeTypeHandle, XmlSerializer>();

        public static T Deserialize<T>(this XDocument xmlDocument)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (XmlReader reader = xmlDocument.CreateReader())
                return (T)xmlSerializer.Deserialize(reader);
        }

        public static string ToXml<T>(this T o) where T : new()
        {
            XDocument doc = new XDocument();
            using (XmlWriter xmlWriter = doc.CreateWriter())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(xmlWriter, o);
                xmlWriter.Close();
            }
            return doc.ToString();
        }

        public static void ToXml<T>(this T value, Stream stream) where T : new()
        {
            var _serializer = GetValue(typeof(T));
            _serializer.Serialize(stream, value);
        }

        public static T FromXml<T>(this string srcString) where T : new()
        {
            var _serializer = GetValue(typeof(T));
            using (var _stringReader = new StringReader(srcString))
            {
                using (XmlReader _reader = new XmlTextReader(_stringReader))
                {
                    return (T)_serializer.Deserialize(_reader);
                }
            }
        }
        public static T FromXml<T>(this Stream source) where T : new()
        {
            var _serializer = GetValue(typeof(T));
            return (T)_serializer.Deserialize(source);
        }

        private static XmlSerializer GetValue(Type type)
        {
            XmlSerializer _serializer;
            if (!ms_serializers.TryGetValue(type.TypeHandle, out _serializer))
            {
                lock (ms_serializers)
                {
                    if (!ms_serializers.TryGetValue(type.TypeHandle, out _serializer))
                    {
                        _serializer = new XmlSerializer(type);
                        ms_serializers.Add(type.TypeHandle, _serializer);
                    }
                }
            }
            return _serializer;
        }
    }
}
