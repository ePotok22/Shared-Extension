using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FFF.Shared
{
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T item, Encoding encoding = null, DataContractJsonSerializer serializer = null)
        {
            encoding = encoding ?? Encoding.Default;
            serializer = serializer ?? new DataContractJsonSerializer(typeof(T));

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, item);
                string json = encoding.GetString(stream.ToArray());

                return json;
            }
        }

    }
}
