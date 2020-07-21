using DotNetUtils.Serialize.Xml;
using System;
using System.Text;

namespace DotNetUtils.Serialize
{
    public static class SerializeExtension
    {
        #region XmlSerialize
        public static string SerializeToXml<T>(this T value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            return XmlSerializerHelper.Instance.SerializeToXml<T>(value, preserveTypeInformation, encoding);
        }

        public static string SerializeToXml(this object value, Type sourceType, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            return XmlSerializerHelper.Instance.SerializeToXml(sourceType, value, preserveTypeInformation, encoding);
        }

        public static T DeserializeFromXml<T>(this string xmlString, Encoding encoding = null)
        {
            return XmlSerializerHelper.Instance.DeserializeFromXml<T>(xmlString, encoding);
        }

        public static object DeserializeFromXml(this string xmlString, Type targetType, Encoding encoding = null)
        {
            return XmlSerializerHelper.Instance.DeserializeFromXml(targetType, xmlString, encoding);
        } 
        #endregion
    }
}
