using DotNetUtils.Serialize.Xml;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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
            //var serializedObject = xmlSerializerHelper.SerializeToXml<object>(null);
            var serializedObject = xmlSerializerHelper.SerializeToXml<object>(null,preserveTypeInformation:true);
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
            //var serializedString = xmlSerializerHelper.SerializeToXml(inputObject);
            var serializedString = xmlSerializerHelper.SerializeToXml(inputObject, preserveTypeInformation: true);
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
            //var serializedString = xmlSerializerHelper.SerializeToXml(inputObject);
            var serializedString = xmlSerializerHelper.SerializeToXml(inputObject, preserveTypeInformation: true);
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
            //var serializedString = xmlSerializerHelper.SerializeToXml(inputList);
            var serializedString = xmlSerializerHelper.SerializeToXml(inputList,preserveTypeInformation:true);
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
            var serializedString = xmlSerializerHelper.SerializeToXml(inputList, preserveTypeInformation: true);
            var deserializedList = xmlSerializerHelper.DeserializeFromXml<IList<string>>(serializedString);

            // Assert
            Assert.That(serializedString, Is.Not.Null);
            Assert.That(deserializedList, Is.Not.Null);
            Assert.That(deserializedList, Has.Count);
            Assert.That(inputList, Is.EquivalentTo(deserializedList));
        }

    }
}