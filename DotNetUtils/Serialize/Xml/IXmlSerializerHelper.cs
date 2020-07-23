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
        Encoding Encoding { get; set; }

        /// <summary>
        /// Serializes XML-serializable objects to XML documents.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="value"></param>
        void SerializeToXml(XmlWriter xmlWriter, object value);

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T">要序列化的类型</typeparam>
        /// <param name="value"></param>
        /// <param name="preserveTypeInformation">指示序列化程序是否保留给定值的原始类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string SerializeToXml<T>(T value, bool preserveTypeInformation=false, Encoding encoding=null);

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="value"></param>
        /// <param name="preserveTypeInformation">指示序列化程序是否保留给定值的原始类型，默认值为false</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        string SerializeToXml(Type sourceType, object value, bool preserveTypeInformation=false, Encoding encoding=null);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T">要反序列化的类型</typeparam>
        /// <param name="xmlString"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        T DeserializeFromXml<T>(string xmlString, Encoding encoding=null);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="xmlString"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        object DeserializeFromXml(Type targetType, string xmlString, Encoding encoding=null);
    }
}
