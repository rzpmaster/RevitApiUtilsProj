using DotNetUtils.Serialize.Xml;
using DotNetUtils.Test.Serialize.TestData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DotNetUtils.Test.Serialize
{
    [TestFixture]
    public class XmlSerializerHelperTest
    {
        [Test]
        public void ShouldThrowArgumentNullExceptionIfObjectIsNull()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;

            // Act
            var serializedObject = xmlSerializerHelper.SerializeToXml<object>(null);
            var deserializedObject = xmlSerializerHelper.DeserializeFromXml<object>(serializedObject);

            // Assert
            Assert.That(serializedObject, Is.Not.Null);
            Assert.That(deserializedObject, Is.Null);
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionIfSourceTypeIsNull()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;

            // Act
            Action action = () => xmlSerializerHelper.DeserializeFromXml(null, string.Empty);

            // Assert
            Assert.That(action, Throws.InstanceOf(typeof(ArgumentNullException)));
        }

        [Test]
        public void ShouldThrowArgumentExceptionIfXmlStringIsNullOrEmpty()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;

            // Act
            Action action = () => xmlSerializerHelper.DeserializeFromXml(typeof(string), string.Empty);

            // Assert
            Assert.That(action, Throws.InstanceOf(typeof(ArgumentException)));
        }

        [Test]
        public void ShouldSerializeEmptyObject()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            object obj = new object();

            // Act
            var serializedString = xmlSerializerHelper.SerializeToXml(obj);
            var deserializedObject = xmlSerializerHelper.DeserializeFromXml<object>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedObject, Is.Not.Null);
        }

        [Test]
        public void ShouldSerializeSimpleObjectWithGenericMethod()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            object inputObject = new SimpleSerializerClass { BoolProperty = true, StringProperty = "test" };

            // Act
            var serializedString = xmlSerializerHelper.SerializeToXml(inputObject);
            var deserializedObject = xmlSerializerHelper.DeserializeFromXml<SimpleSerializerClass>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedObject, Is.Not.Null);
            Assert.That(inputObject, Is.EqualTo(deserializedObject));
        }

        [Test]
        public void ShouldSerializeSimpleObjectWithNonGenericMethod()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            object inputObject = new SimpleSerializerClass { BoolProperty = true, StringProperty = "test" };
            Type targetType = inputObject.GetType();

            // Act
            var serializedString = xmlSerializerHelper.SerializeToXml(inputObject);
            var deserializedObject = (SimpleSerializerClass)xmlSerializerHelper.DeserializeFromXml(targetType, serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedObject, Is.Not.Null);
            Assert.That(inputObject, Is.EqualTo(deserializedObject));
        }

        [Test]
        public void ShouldSerializeConcreteList()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            List<string> inputList = new List<string> { "a", "b", "c" };

            // Act
            var serializedString = xmlSerializerHelper.SerializeToXml(inputList);
            var deserializedList = xmlSerializerHelper.DeserializeFromXml<List<string>>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedList, Is.Not.Null);
            Assert.That(deserializedList, Has.Count);
            Assert.That(inputList, Is.EquivalentTo(deserializedList));
        }

        [Test]
        public void ShouldSerializeInterfaceList()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            IList<string> inputList = new List<string> { "a", "b", "c" };

            // Act
            // preserveTypeInformation 记录 接口类型 的真实类型 认真调试下本条
            var serializedString = xmlSerializerHelper.SerializeToXml(inputList, preserveTypeInformation: true);
            var deserializedList = xmlSerializerHelper.DeserializeFromXml<IList<string>>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedList, Is.Not.Null);
            Assert.That(deserializedList, Has.Count);
            Assert.That(inputList, Is.EquivalentTo(deserializedList));
        }

        [Test]
        public void ShouldSerializeNullableValue()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;

            // Act
            // 观察 preserveTypeInformation 参数 记录非接口 的真实类型
            var serializedString = xmlSerializerHelper.SerializeToXml<int?>(null, preserveTypeInformation: true);
            var deserialized = xmlSerializerHelper.DeserializeFromXml<int?>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserialized, Is.Null);
        }

        [Test]
        public void ShouldDeserializeListFromXmlFile()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            var restaurantsXml = File.ReadAllText("./DotNetUtils.Test/Serialize/TestData/SerializedData.xml");
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var listOfRestaurants = xmlSerializerHelper.DeserializeFromXml<List<Restaurant>>(restaurantsXml);
            stopwatch.Stop();

            // Assert
            Assert.That(listOfRestaurants, Has.Count.EqualTo(4891));
            Assert.That(stopwatch.Elapsed.TotalMilliseconds, Is.LessThanOrEqualTo(1500));
        }

        [Test]
        public void ShouldDeserializeXmlWithEncoding()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            string serializedString = @"<?xml version=""1.0"" encoding=""iso-8859-1"" ?><SimpleSerializerClass><StringProperty>6.00% p.a. Multi Barrier Reverse Convertible on EURO STOXX 50® Index, S&amp;P 500®, Swiss Market Index®</StringProperty></SimpleSerializerClass>";
            var encoding = Encoding.GetEncoding("ISO-8859-1");

            // Act
            var deserializedObject = xmlSerializerHelper.DeserializeFromXml<SimpleSerializerClass>(serializedString, encoding);

            // Assert
            Assert.That(deserializedObject, Is.Not.Null);
            Assert.That(deserializedObject.StringProperty, Does.Not.Contain("Â"), "This character is only contained if the wrong encoding is used.");
            Assert.That(xmlSerializerHelper.Encoding, Is.EqualTo(Encoding.UTF8));
        }

        [Test]
        public void ShouldDeserializeXmlWithEncodingMismatch()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            xmlSerializerHelper.Encoding = Encoding.UTF8;
            string serializedString = @"<?xml version=""1.0"" encoding=""iso-8859-1"" ?><SimpleSerializerClass><StringProperty>6.00% p.a. Multi Barrier Reverse Convertible on EURO STOXX 50® Index, S&amp;P 500®, Swiss Market Index®</StringProperty></SimpleSerializerClass>";

            // Act
            var deserializedObject = xmlSerializerHelper.DeserializeFromXml<SimpleSerializerClass>(serializedString);

            // Assert
            Assert.That(deserializedObject, Is.Not.Null);
            Assert.That(deserializedObject.StringProperty, Does.Contain("Â"));
        }

        [Test]
        public void ShouldSerializeToXmlDocument()
        {
            // Arrange
            IXmlSerializerHelper xmlSerializerHelper = XmlSerializerHelper.Instance;
            var students = new Students
            {
                Student = new[]
                {
                    new StudentsStudent
                        {
                            RollNo = 1,
                            Name = "Thomas",
                            Address = "6330 Cham"
                        }
                }
            };

            // Act
            var serializedStudents = xmlSerializerHelper.SerializeToXmlDocument(students);
            var deserializedStudents = xmlSerializerHelper.DeserializeFromXml<Students>(serializedStudents);

            // Assert
            Assert.That(serializedStudents, Is.Not.Null);
            Assert.That(deserializedStudents, Is.Not.Null);
            Assert.That(deserializedStudents.Student, Has.Length.EqualTo(students.Student.Length));
        }

    }
}