using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils
{
    /// <summary>
    /// View Class is Inherited from Element
    /// and its subclass has
    /// TableView
    ///     PanelScheduleView   配电盘明细表视图
    ///     ViewSchedule        明细表视图
    /// View3D                  三维视图
    /// ViewDrafting
    ///     ImageView           图片视图
    /// ViewPlan                平面视图
    /// ViewSection             剖面视图
    /// ViewSheet               视图页
    /// </summary>
    public static class ViewUtils
    {
        /// <summary>
        /// 跳转试图
        /// </summary>
        /// <param name="UIDoc"></param>
        /// <param name="view"></param>
        public static void SwitchView(UIDocument uidoc, View view)
        {
            if (null == uidoc || null == view)
            {
                return;
            }

            //跳转视图
            uidoc.ActiveView = view;
        }

        /// <summary>
        /// 高亮给定的元素
        /// </summary>
        /// <param name="UIDoc"></param>
        /// <param name="elementsToHighLight"></param>
        public static void HighLightElements(UIDocument uidoc, List<ElementId> elementsToHighLight)
        {
            if (null == uidoc)
            {
                return;
            }

            ICollection<ElementId> elementIds = uidoc.Selection.GetElementIds();
            elementIds.Clear();
            for (int i = 0; i < elementsToHighLight.Count(); i++)
            {
                elementIds.Add(elementsToHighLight[i]);
            }
            uidoc.Selection.SetElementIds(elementIds);
        }

        /// <summary>
        /// Zoom给定的元素
        /// </summary>
        /// <param name="UIDoc"></param>
        /// <param name="elements"></param>
        /// <param name="zoomFactor">0-1之间，默认为0.8，数值越小给定元素在频幕中的占比越小</param>
        public static void ZoomAndFitElements(UIDocument uidoc, List<Element> elements, Double zoomFactor = 0.8)
        {
            UIView uiView = GetUIView(uidoc);

            if (null == uiView)
            {
                return;
            }

            if (null != elements)
            {
                BoundingBoxXYZ bbox = BoundingBoxUtils.GetElementsMaxBounding(elements);

                uiView.ZoomAndCenterRectangle(bbox.Max, bbox.Min);
            }

            uiView.Zoom(zoomFactor);
        }

        private static UIView GetUIView(UIDocument uidoc)
        {
            IList<UIView> UIViews = uidoc.GetOpenUIViews();
            var activeViewId = uidoc.Document.ActiveView.Id;

            UIView uiView = null;

            for (int i = 0; i < UIViews.Count(); i++)
            {
                if (UIViews[i].ViewId == activeViewId)
                {
                    uiView = UIViews[i];
                    break;
                }
            }

            return uiView;
        }

        /// <summary>
        /// 获取剖面视图在平面视图上的范围框 CurveLoop（可能又一定的倾斜角度）, Z = minZ
        /// </summary>
        /// <param name="viewSection"></param>
        /// <returns></returns>
        public static CurveLoop GetViewSectionPlaneLoop(ViewSection viewSection)
        {
            var pts = GetViewSectionPlanePointsLoop(viewSection);
            return pts.ToCurveLoop();
        }

        /// <summary>
        /// 获取剖面视图在平面视图上的范围框的点的序列, Z = minZ
        /// </summary>
        /// <param name="viewSection"></param>
        /// <returns></returns>
        public static List<XYZ> GetViewSectionPlanePointsLoop(this ViewSection viewSection)
        {
            BoundingBoxXYZ bbox = viewSection.CropBox;
            Double farCropDist = viewSection.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).AsDouble();

            var direct = viewSection.ViewDirection.Normalize();
            var minPt = bbox.Transform.OfPoint(bbox.Min);
            var maxPt = bbox.Transform.OfPoint(bbox.Max);

            var p1 = new XYZ(minPt.X, minPt.Y, minPt.Z);
            var p3 = new XYZ(maxPt.X, maxPt.Y, minPt.Z);
            var p2 = p3 - direct * farCropDist;
            var p4 = p1 + direct * farCropDist;

            return new List<XYZ> { p1, p2, p3, p4 };
        }
    }
}
