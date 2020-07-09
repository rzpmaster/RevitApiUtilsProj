using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

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
            return GetRevitLinkInstance(currDoc, linkedRef).GetLinkDocument();
        }

        /// <summary>
        /// 获取链接文件实例
        /// </summary>
        /// <param name="currDoc"></param>
        /// <param name="linkedRef"></param>
        /// <returns></returns>
        public static RevitLinkInstance GetRevitLinkInstance(Document currDoc, Reference linkedRef)
        {
            if (linkedRef.LinkedElementId == ElementId.InvalidElementId) return null;

            string stableReflink = linkedRef.ConvertToStableRepresentation(currDoc).Split(':')[0];
            Reference refLink = Reference.ParseFromStableRepresentation(currDoc, stableReflink);
            return currDoc.GetElement(refLink) as RevitLinkInstance;
        }

        /// <summary>
        /// 获取链接文件实例
        /// </summary>
        /// <param name="currDoc"></param>
        /// <param name="linkedElement"></param>
        /// <returns></returns>
        public static RevitLinkInstance GetRevitLinkInstance(Document currDoc, Element linkedElement)
        {
            if (!linkedElement.Document.IsLinked) return null;

            FilteredElementCollector collector = new FilteredElementCollector(currDoc);
            var linkInstances = collector.OfClass(typeof(RevitLinkInstance)).ToElements().Cast<RevitLinkInstance>();
            foreach (RevitLinkInstance linkInstance in linkInstances)
            {
                Document document = linkInstance.GetLinkDocument();
                if (document.Title == linkedElement.Document.Title) return linkInstance;
            }

            return null;
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
