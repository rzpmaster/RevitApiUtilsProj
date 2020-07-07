using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitUtils
{
    /// <summary>
    /// 链接文件中的房间的选择过滤器
    /// </summary>
    public class LinkRoomSelectionFilter : ISelectionFilter
    {
        Document linkDoc = null;

        public bool AllowElement(Element elem)
        {
            if (elem is RevitLinkInstance)
            {
                linkDoc = (elem as RevitLinkInstance).GetLinkDocument();
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (linkDoc != null)
            {
                var r = linkDoc.GetElement(reference.LinkedElementId);
                if (r is Room)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
