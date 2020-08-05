using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Serialize.Json
{
    public class JsonSerializerHelper : IJsonSerializerHelper
    {
        static readonly object lockObj = new object();
        static IJsonSerializerHelper implementation;

        public static IJsonSerializerHelper Instance
        {
            get
            {
                if (implementation == null)
                {
                    lock (lockObj)
                    {
                        if (implementation == null)
                        {
                            implementation = new JsonSerializerHelper();
                        }
                    }
                }

                return implementation;
            }
        }

        private JsonSerializerHelper()
        {
            this.DefaultEncoding = Encoding.UTF8;
        }

        #region IJsonSerializerHelper Members
        /// <summary>
        /// 默认为 Encoding.UTF8
        /// </summary>
        public Encoding DefaultEncoding { get; set; }

        public object DeserializeFromJson(Type targetType, string jsonString, Encoding encoding = null)
        {
            var serializer = new JsonSerializer();

            encoding = encoding ?? this.DefaultEncoding;
            byte[] buffer = encoding.GetBytes(jsonString);

            using (var memoryStream = new MemoryStream(buffer))
            using (var streamReader = new StreamReader(memoryStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                var deserialized = serializer.Deserialize(reader, targetType);
                return deserialized;
            }
        }

        public T DeserializeFromJson<T>(string jsonString, Encoding encoding = null)
        {
            Type targetType = typeof(T);
            return (T)this.DeserializeFromJson(targetType, jsonString, encoding);
        }

        public string SerializeToJson(Type sourceType, object value, Encoding encoding = null)
        {
            encoding = encoding ?? this.DefaultEncoding;
            var serializer = new JsonSerializer();

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, encoding))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(streamWriter, value);
                byte[] buffer = (streamWriter.BaseStream as MemoryStream).ToArray();
                string json = encoding.GetString(buffer, 0, buffer.Length);
                return json;
            }
        }

        public string SerializeToJson<T>(T value, Encoding encoding = null)
        {
            return this.SerializeToJson(typeof(T), value, encoding);
        }
        #endregion
    }
}
