using System;
using System.IO;
using System.Linq;
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
            this.DefaultEncoding = Encoding.UTF8;
        }

        #region IXmlSerializerHelper Members
        /// <summary>
        /// 默认为 Encoding.UTF8
        /// </summary>
        public Encoding DefaultEncoding { get; set; }

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

            encoding = encoding ?? this.DefaultEncoding;
            byte[] buffer = encoding.GetBytes(xmlString);

            if (!ValueToTypeMapping.CheckIfStringContainsTypeInformation(xmlString))
            {
                var serializer = new XmlSerializer(targetType);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    var deserialized = serializer.Deserialize(memoryStream);
                    return deserialized;
                }
            }

            bool isTargetTypeAnInterface = targetType.GetTypeInfo().IsInterface;
            Type[] extraTypes = { };
            if (!isTargetTypeAnInterface)
            {
                extraTypes = new[] { targetType };
            }

            var serializerBefore = new XmlSerializer(typeof(ValueToTypeMapping), extraTypes);
            ValueToTypeMapping deserializedObject = null;

            using (var memoryStream = new MemoryStream(buffer))
            {
                deserializedObject = (ValueToTypeMapping)serializerBefore.Deserialize(memoryStream);
            }

            // If the target type is an interface, we need to deserialize again with more type information
            if (isTargetTypeAnInterface)
            {
                // 依靠 ValueToTypeMapping 中的 TypeName 判断真实类型 
                Type serializedType = Type.GetType(deserializedObject.TypeName);

                // 重新反序列化
                var serializerAfter = new XmlSerializer(typeof(ValueToTypeMapping), new[] { serializedType });
                using (var memoryStream = new MemoryStream(buffer))
                {
                    deserializedObject = (ValueToTypeMapping)serializerAfter.Deserialize(memoryStream);
                }

                return Convert.ChangeType(deserializedObject.Value, serializedType);
            }

            return deserializedObject.Value;
        }

        public T DeserializeFromXml<T>(string xmlString, Encoding encoding = null)
        {
            Type targetType = typeof(T);
            return (T)this.DeserializeFromXml(targetType, xmlString, encoding);
        }

        public string SerializeToXml(Type sourceType, object value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            encoding = encoding ?? this.DefaultEncoding;

            if (sourceType.GetTypeInfo().IsInterface && value != null)
            {
                sourceType = value.GetType();
            }

            object objectToSerialize;
            if (preserveTypeInformation)
            {
                objectToSerialize = new ValueToTypeMapping
                {
                    Value = value,
                    TypeName = sourceType.FullName
                    //TypeName = sourceType.Name // 必须全名，否则找不到
                };
            }
            else
            {
                objectToSerialize = value;
            }

            var mainType = objectToSerialize?.GetType() ?? sourceType;
            var extraTypes = new[] { sourceType };
            var serializer = new XmlSerializer(mainType, extraTypes);
            // 去掉 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" 属性
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(
                new XmlQualifiedName[] {
                        XmlQualifiedName.Empty
             });

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, encoding))
                {
                    serializer.Serialize(streamWriter, objectToSerialize, namespaces);
                    byte[] buffer = (streamWriter.BaseStream as MemoryStream).ToArray();
                    string xml = encoding.GetString(buffer, 0, buffer.Length);
                    return xml;
                }
            }
        }

        public string SerializeToXml<T>(T value, bool preserveTypeInformation = false, Encoding encoding = null)
        {
            return this.SerializeToXml(typeof(T), value, preserveTypeInformation, encoding);
        }
        #endregion

        /// <summary>
        /// 序列化为 xmlWriter
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
        /// 序列化为  XmlDocument
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string SerializeToXmlDocument(object value, Encoding encoding = null)
        {
            var doc = new XmlDocument();
            var nav = doc.CreateNavigator();

            // 写入 XmlDocument
            using (var xmlWriter = nav.AppendChild())
            {
                this.SerializeToXml(xmlWriter, value);
            }

            //return doc.OuterXml;

            // 从 XmlDocument 写入 字符串
            encoding = encoding ?? this.DefaultEncoding;

            using (var stringWriter = new StringWriterWithEncoding(encoding))
            {
                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                    Encoding = encoding
                };

                using (var xmlTextWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }

        /// <summary>
        /// 记录 ValueToType 的映射
        /// </summary>
        public class ValueToTypeMapping
        {
            /// <summary>
            /// 表示Id，用来判断 xml 是否 是以 ValueToTypeMapping 类型序列化的
            /// </summary>
            public string[] Id { get => identifiers; }

            /// <summary>
            /// 记录真实 类型信息
            /// </summary>
            public string TypeName { get; set; }

            /// <summary>
            /// 记录 真实值
            /// </summary>
            public object Value { get; set; }

            public static bool CheckIfStringContainsTypeInformation(string xmlString)
            {
                return identifiers.All(id => xmlString.Contains(id));
            }

            /// <summary>
            /// 标识字符串，如果以 ValueToTypeMapping 类型序列化，以下字符必然会出现在 xml 中
            /// </summary>
            static readonly string[] identifiers = new string[]
            {
                "<ValueToTypeMapping>",
                "<TypeName>",
                "<Value"
            };
        }
    }

    class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }
    }
}
