using System;
using System.Text;
using System.Xml;

namespace DotNetUtils.Serialize.Xml
{
    public interface IXmlSerializerHelper
    {
        /// <summary>
        /// The encoding used to serialize and deserialize xml strings.
        /// </summary>
        Encoding DefaultEncoding { get; set; }

        string SerializeToXml(Type sourceType, object value, bool preserveTypeInformation = false, Encoding encoding = null);

        string SerializeToXml<T>(T value, bool preserveTypeInformation = false, Encoding encoding = null);

        object DeserializeFromXml(Type targetType, string xmlString, Encoding encoding = null);

        T DeserializeFromXml<T>(string xmlString, Encoding encoding = null);
    }
}
