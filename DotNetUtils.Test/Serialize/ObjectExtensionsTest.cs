using DotNetUtils.Serialize;
using NUnit.Framework;
using System;

namespace DotNetUtils.Test.Serialize
{
    [TestFixture]
    public class ObjectExtensionsTest
    {
        [Test]
        public void ShouldTestSerializeToXmlExtensionMethod()
        {
            // Arrange
            float inputValue = 123.456f;
            string expectedOuput = @"﻿<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine + "<float>123.456</float>";

            // Acts
            var serializedString = inputValue.SerializeToXml();

            // Assert
            Assert.That(serializedString, Is.EqualTo(expectedOuput));
        }

        [Test]
        public void ShouldTestDeserializeFromXmlExtensionMethodGeneric()
        {
            // Arrange
            string serializedString = @"﻿<?xml version=""1.0"" encoding=""utf-8""?><float>123.456</float>";
            float expectedOuput = 123.456f;

            // Act
            var deserializedObject = serializedString.DeserializeFromXml<float>();

            // Assert
            Assert.That(deserializedObject, Is.EqualTo(expectedOuput));
        }

        [Test]
        public void ShouldTestDeserializeFromXmlExtensionMethodNonGeneric()
        {
            // Arrange
            string serializedString = @"﻿<?xml version=""1.0"" encoding=""utf-8""?><float>123.456</float>";
            float expectedOuput = 123.456f;

            // Act
            var deserializedObject = serializedString.DeserializeFromXml(typeof(float));

            // Assert
            Assert.That(deserializedObject, Is.EqualTo(expectedOuput));
        }
    }
}
