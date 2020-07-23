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
            {//如果没有标识类型信息，说明没有用ValueToTypeMapping序列化，直接反序列化即可
                var serializer = new XmlSerializer(targetType);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    var deserialized = serializer.Deserialize(memoryStream);
                    return deserialized;
                }
            }

            ValueToTypeMapping deserializedObject = null;
            // 第一次尝试序列化,只能看到他的类型信息
            var serializerBefore = new XmlSerializer(typeof(ValueToTypeMapping));
            using (var memoryStream = new MemoryStream(buffer))
            {
                deserializedObject = (ValueToTypeMapping)serializerBefore.Deserialize(memoryStream);
            }
            Type serializedType = Type.GetType(deserializedObject.TypeName);
            // 得到类型信息后，第二次彻底序列化
            var serializerAfter = new XmlSerializer(typeof(ValueToTypeMapping), new[] { serializedType });
            using (var memoryStream = new MemoryStream(buffer))
            {
                deserializedObject = (ValueToTypeMapping)serializerAfter.Deserialize(memoryStream);
            }

            return Convert.ChangeType(deserializedObject.Value, serializedType);
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
        /// 序列化
        /// </summary>
        /// <param name="sourceType">要序列化的类型</param>
        /// <param name="value"></param>
        /// <param name="preserveTypeInformation">指示序列化程序是否保留给定值的原始类型（只为接口服务）</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SerializeToXml(Type sourceType, object value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            encoding = encoding ?? this.Encoding;

            if (!sourceType.GetTypeInfo().IsInterface && value != null && sourceType != value.GetType())
            {//如果不是接口的原因，源类型和 value 类型不一样，后面绝对写不了，强制不让记录原始类型
                preserveTypeInformation = false;
            }

            if (value == null)
            {//如果值为null，记录类型没有意义，也强制不让记录
                preserveTypeInformation = false;
            }

            if (sourceType.GetTypeInfo().IsInterface && value != null)
            {//如果是接口类型，需要更新 接口的实际类型
                sourceType = value.GetType();
            }

            object objectToSerialize;
            if (preserveTypeInformation)
            {//如果需要保留原始信息，则需要记录一下值和类型的映射，最后序列化这个映射对象
                objectToSerialize = new ValueToTypeMapping
                {
                    Value = value,
                    TypeName = value.GetType().FullName
                };
            }
            else
            {
                objectToSerialize = value;
            }

            // 注意 如果 preserveTypeInformation 参数为 true 
            // mianType 为   ValueToTypeMapping
            // extraTypes 为 真实类型 sourceType（只有当他是接口的时候才会取更新它，否则就是传进来的值，如果传进来的值是父类，而且他又要记录原始的类型，下面后无法序列化会报错，所以上面才会强制修改 preserveTypeInformation 参数）
            var mainType = objectToSerialize?.GetType() ?? sourceType;
            var extraTypes = new[] { sourceType };

            // 初始化带有类型信息的序列化器，会将类型信息系列化到 xml 字符串中
            var serializer = new XmlSerializer(mainType, extraTypes);

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, encoding))
                {
                    serializer.Serialize(streamWriter, objectToSerialize);
                    // 当 mainType 无法单独完成序列化（为 ValueToTypeMapping 类型时）
                    // 并且 objectToSerialize 类型和 extraTypes 数组不一样，或者 extraTypes 是父类时，会报错，无法序列化

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

            /// <summary>
            /// 标识字符串，如果以 ValueToTypeMapping 类型序列化，以下字符必然会出现在 xml 中
            /// </summary>
            static readonly string identifier = "<ValueToTypeMapping xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
        }
    }
}
