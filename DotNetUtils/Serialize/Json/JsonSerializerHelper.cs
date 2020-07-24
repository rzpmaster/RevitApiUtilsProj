using System;
using System.Collections.Generic;
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
        public Encoding DefaultEncoding { get; set; }

        public object DeserializeFromJson(Type targetType, string jsonString, Encoding encoding = null)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromJson<T>(string jsonString, Encoding encoding = null)
        {
            throw new NotImplementedException();
        }

        public string SerializeToJson(Type sourceType, object value, Encoding encoding = null)
        {
            throw new NotImplementedException();
        }

        public string SerializeToJson<T>(T value, Encoding encoding = null)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
