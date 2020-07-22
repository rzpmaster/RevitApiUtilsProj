using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DotNetUtils.Serialize.Xml
{
    public class XmlSerializerHelper : IXmlSerializerHelper
    {
        static readonly object lockObj = new object();
        static IXmlSerializerHelper implementation;

        public static IXmlSerializerHelper Instance
        {
            get
            {
                if (implementation == null)
                {
                    lock (lockObj)
                    {
                        if (implementation == null)
                        {
                            implementation = new XmlSerializerHelper();
                        }
                    }
                }

                return implementation;
            }
        }

        private XmlSerializerHelper()
        {
            this.Encoding = Encoding.UTF8;
        }

        #region IXmlSerializerHelper Members
        /// <summary>
        /// 默认为 Encoding.UTF8
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public T DeserializeFromXml<T>(string xmlString, Encoding encoding = null)
        {
            Type targetType = typeof(T);
            return (T)this.DeserializeFromXml(targetType, xmlString, encoding);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="xmlString"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public object DeserializeFromXml(Type targetType, string xmlString, Encoding encoding = null)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (string.IsNullOrEmpty(xmlString))
            {
                throw new ArgumentException("Must not be null or empty", nameof(xmlString));
            }

            encoding = encoding ?? this.Encoding;
            byte[] buffer = encoding.GetBytes(xmlString);

            if (!ValueToTypeMapping.CheckIfStringContainsTypeInformation(xmlString))
            {//如果没有表示类型信息,直接用普通方法反序列化
                var serializer = new XmlSerializer(targetType);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    var deserialized = serializer.Deserialize(memoryStream);
                    return deserialized;
                }
            }

            ValueToTypeMapping deserializedObject = null;
            // 第一次尝试序列化
            var serializerBefore = new XmlSerializer(typeof(ValueToTypeMapping));
            using (var memoryStream = new MemoryStream(buffer))
            {
                deserializedObject = (ValueToTypeMapping)serializerBefore.Deserialize(memoryStream);
            }

            bool isTargetTypeAnInterface = targetType.GetTypeInfo().IsInterface;
            if (!isTargetTypeAnInterface)
            {//如果不是接口，直接返回
                return deserializedObject.Value;
            }
            else
            {//如果目标类型是接口，则需要使用更多类型信息再次反序列化
                Type serializedType = Type.GetType(deserializedObject.TypeName);
                var serializerAfter = new XmlSerializer(typeof(ValueToTypeMapping), new[] { serializedType });
                using (var memoryStream = new MemoryStream(buffer))
                {
                    deserializedObject = (ValueToTypeMapping)serializerAfter.Deserialize(memoryStream);
                }

                return Convert.ChangeType(deserializedObject.Value, serializedType);
            }
        }

        /// <summary>
        /// 将 object 序列化为 xmlWriter
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="value"></param>
        public void SerializeToXml(XmlWriter xmlWriter, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var serializer = new XmlSerializer(value.GetType());
            serializer.Serialize(xmlWriter, value);
        }

        /// <summary>
        /// 将对象序列化为 xml 字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="preserveTypeInformation"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SerializeToXml<T>(T value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            return this.SerializeToXml(typeof(T), value, preserveTypeInformation, encoding);
        }

        /// <summary>
        /// 将对象序列化为 xml 字符串
        /// </summary>
        /// <param name="sourceType">当前序列化对象的准确运行时类型</param>
        /// <param name="value"></param>
        /// <param name="preserveTypeInformation">指示序列化程序是否保留给定值的原始类型</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SerializeToXml(Type sourceType, object value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            encoding = encoding ?? this.Encoding;

            if (sourceType.GetTypeInfo().IsInterface && value != null)
            {//如果是接口类型，需要获取接口类型实例的类型
                sourceType = value.GetType();
            }

            object objectToSerialize;
            if (preserveTypeInformation)
            {//如果需要保留原始信息，则需要记录一下值和类型的映射，最后序列化这个映射对象
                objectToSerialize = new ValueToTypeMapping
                {
                    Value = value,
                    TypeName = sourceType.FullName
                };
            }
            else
            {
                objectToSerialize = value;
            }

            var mainType = objectToSerialize?.GetType() ?? sourceType;
            var extraTypes = new[] { sourceType };

            // 注意 如果 preserveTypeInformation 参数为 true 这里的 mianType 为 ValueToTypeMapping ，extraTypes 为真实类型
            var serializer = new XmlSerializer(mainType, extraTypes);

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, encoding))
                {
                    serializer.Serialize(streamWriter, objectToSerialize);
                    byte[] buffer = (streamWriter.BaseStream as MemoryStream).ToArray();
                    string xml = encoding.GetString(buffer, 0, buffer.Length);
                    return xml;
                }
            }
        }
        #endregion

        /// <summary>
        /// 记录 ValueToType 的映射
        /// </summary>
        public class ValueToTypeMapping
        {
            public string Id { get => identifier; }

            public string TypeName { get; set; }

            public object Value { get; set; }

            public static bool CheckIfStringContainsTypeInformation(string xmlString)
            {
                return xmlString.Contains(identifier);
            }

            static readonly string identifier = "ValueToTypeMapping_663FBB7F-9C0A-400C-A9C4-76ACADE8C741";
        }
    }
}
