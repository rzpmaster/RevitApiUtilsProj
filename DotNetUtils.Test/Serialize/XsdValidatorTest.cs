using DotNetUtils.Serialize.Xml;
using DotNetUtils.Test.Serialize.TestData;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Test.Serialize
{
    [TestFixture]
    public class XsdValidatorTest
    {
        [Test]
        public void ShouldValidateXsdSchemaSuccessfully()
        {
            // Arrange
            string xmlContent = XmlTestData.GetValidXmlContent();
            string xsdContent = XmlTestData.GetXsdMarkup();

            IXsdValidator xsdValidator = XsdValidator.Instance;

            // Act
            var validationResult = xsdValidator.Validate(xmlContent, xsdContent);

            // Assert
            Assert.That(validationResult.IsValid, Is.True);
        }

        [Test]
        public void ShouldFailToValidateXsdSchema()
        {
            // Arrange
            string xmlContent = XmlTestData.GetInvalidXmlContent();
            string xsdContent = XmlTestData.GetXsdMarkup();

            IXsdValidator xsdValidator = XsdValidator.Instance;

            // Act
            var validationResult = xsdValidator.Validate(xmlContent, xsdContent);

            // Assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Count.EqualTo(1));
            // var meg = validationResult.Errors.ElementAt(0).Message;
        }

        [Test]
        public void ShouldAccessStaticInstance()
        {
            // Act
            IXsdValidator xsdValidator = XsdValidator.Instance;

            // Assert
            Assert.That(xsdValidator,Is.Not.Null);
        }
    }
}
