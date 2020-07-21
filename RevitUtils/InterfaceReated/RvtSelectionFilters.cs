using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitUtils.InterfaceReated
{
    /// <summary>
    /// 链接文件中的元素选择过滤器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedElementSelectionFilter<T> : ISelectionFilter
        where T : Element
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
                if (r is T)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 当前文件中的元素选择过滤器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElementSelectionFilter<T> : ISelectionFilter
        where T : Element
    {
        public bool AllowElement(Element elem)
        {
            if (elem is T)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
