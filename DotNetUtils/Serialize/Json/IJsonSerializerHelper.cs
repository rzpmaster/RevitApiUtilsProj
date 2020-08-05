using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Serialize.Json
{
    public interface IJsonSerializerHelper
    {
        /// <summary>
        /// The encoding used to serialize and deserialize json strings.
        /// </summary>
        Encoding DefaultEncoding { get; set; }

        string SerializeToJson(Type sourceType, object value, Encoding encoding = null);

        string SerializeToJson<T>(T value, Encoding encoding = null);

        object DeserializeFromJson(Type targetType, string jsonString, Encoding encoding = null);

        T DeserializeFromJson<T>(string jsonString, Encoding encoding = null);
    }
}
