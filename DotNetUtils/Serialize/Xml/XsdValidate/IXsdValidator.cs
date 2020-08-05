namespace DotNetUtils.Serialize.Xml.XsdValidate
{
    public interface IXsdValidator
    {
        /// <summary>
        /// 验证 xml 是否苏荷 xsd
        /// </summary>
        /// <param name="xmlContent">The XML content</param>
        /// <param name="xsdContent">The XSD schema</param>
        /// <returns></returns>
        XsdValidationResult Validate(string xmlContent, string xsdContent);
    }
}
