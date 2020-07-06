using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitUtils
{
    public static class LinkedElementUtils
    {
        /// <summary>
        /// 获得给定链接ref的Document
        /// </summary>
        /// <param name="currDoc"></param>
        /// <param name="linkedRef"></param>
        /// <returns></returns>
        public static Document GetLinkedDocumnet(Document currDoc, Reference linkedRef)
        {
            string stableReflink = linkedRef.ConvertToStableRepresentation(currDoc).Split(':')[0];
            Reference refLink = Reference.ParseFromStableRepresentation(currDoc, stableReflink);
            RevitLinkInstance rli_return = currDoc.GetElement(refLink) as RevitLinkInstance;
            Document linkdoc = rli_return.GetLinkDocument();

            return linkdoc;
        }

        /// <summary>
        /// 获得当前文件中的所有链接文件
        /// </summary>
        /// <param name="currDoc"></param>
        /// <returns></returns>
        public static List<Document> GetAllLinkedDocument(Document currDoc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(currDoc);
            var linkInstances = collector.OfClass(typeof(RevitLinkInstance));

            List<Document> documents = new List<Document>();
            foreach (RevitLinkInstance item in linkInstances)
            {
                Document document = item.GetLinkDocument();
                documents.Add(document);
            }
            return documents;
        }
    }
}
