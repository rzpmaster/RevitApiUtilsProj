using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace DotNetUtils.Serialize.Xml.XsdValidate
{
    public class XsdValidator : IXsdValidator
    {
        static readonly object lockObj = new object();
        static IXsdValidator implementation;

        public static IXsdValidator Instance
        {
            get
            {
                if (implementation == null)
                {
                    lock (lockObj)
                    {
                        if (implementation == null)
                        {
                            implementation = new XsdValidator();
                        }
                    }
                }

                return implementation;
            }
        }

        private XsdValidator()
        {
        }

        public XsdValidationResult Validate(string xmlContent, string xsdContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                throw new ArgumentNullException(nameof(xmlContent));
            }

            if (string.IsNullOrEmpty(xsdContent))
            {
                throw new ArgumentNullException(nameof(xsdContent));
            }

            var validationExceptions = new List<XsdValidationException>();
            var readerSettings = GetXmlReaderSettings(
                xsdContent: xsdContent,
                validationFunction: (obj, eventArgs) => validationExceptions.Add(ToXsdValidationException(eventArgs.Exception)));

            using (var objXmlReader = XmlReader.Create(new StringReader(xmlContent), readerSettings))
            {
                try
                {
                    while (objXmlReader.Read())
                    {
                    }
                }
                catch (XmlSchemaException exception)
                {
                    validationExceptions.Add(ToXsdValidationException(exception));
                }
            }

            return new XsdValidationResult(validationExceptions);
        }

        private static XmlReaderSettings GetXmlReaderSettings(string xsdContent, Action<object, ValidationEventArgs> validationFunction)
        {
            var schema = XmlSchema.Read(
                reader: new StringReader(xsdContent),
                validationEventHandler: (obj, eventArgs) => validationFunction(obj, eventArgs));

            var readerSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            readerSettings.Schemas.Add(schema);

            return readerSettings;
        }

        private static XsdValidationException ToXsdValidationException(XmlSchemaException exception)
        {
            return new XsdValidationException(exception.Message, exception.SourceUri, exception.LineNumber, exception.LinePosition);
        }
    }

    public class XsdValidationResult
    {
        public XsdValidationResult(IEnumerable<XsdValidationException> errors)
        {
            this.Errors = errors.ToList();
        }

        public IEnumerable<XsdValidationException> Errors { get; }

        public bool IsValid
        {
            get
            {
                return !this.Errors.Any();
            }
        }
    }

}
