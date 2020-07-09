using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    public static class RevitExtensions
    {
        /// <summary>
        /// 通过Solid 使用BoundingBoxIntersectsFilter 过滤元素
        /// 速度慢,但是比较准确
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="builtInCategory"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<Element> GetElementsBySolid(this Document document, Solid solid, BuiltInCategory builtInCategory, bool inverted = false)
        {
            if (solid == null) return new List<Element>();

            var collector = new FilteredElementCollector(document);
            var solidFilter = new ElementIntersectsSolidFilter(solid, inverted);
            var list = collector.OfCategory(builtInCategory)
                                .WhereElementIsNotElementType()
                                .WherePasses(solidFilter).ToElements();
            return list;
        }

        /// <summary>
        /// 通过bbox 使用BoundingBoxIntersectsFilter 过滤元素
        /// 速度快，但是不够准确
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="builtInCategory">目标构建的类型</param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<Element> GetElementsByBbox(this Document document, Outline outline, BuiltInCategory builtInCategory, bool inverted = false)
        {
            if (outline == null) return new List<Element>();

            FilteredElementCollector collector = new FilteredElementCollector(document);
            var bboxFilter = new BoundingBoxIntersectsFilter(outline, inverted);
            var list = collector.OfCategory(builtInCategory)
                                .WhereElementIsNotElementType()
                                .WherePasses(bboxFilter).ToElements();
            return list;
        }

        #region FilledRegion
        /// <summary>
        /// 生成填充区域
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="loop"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FilledRegion CreateRangeBoxByFilledRegion(Document doc, List<CurveLoop> loop, String name = "")
        {
            UIDocument uIDocument = new UIDocument(doc);
            var typeId = GetDefultFilledRegionTypeId(doc);
            var viewId = uIDocument.ActiveView.Id;

            return CreateRangeBoxByFilledRegion(doc, typeId, viewId, loop);
        }

        /// <summary>
        /// 生成填充区域
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="typeId"></param>
        /// <param name="viewId"></param>
        /// <param name="loop"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FilledRegion CreateRangeBoxByFilledRegion(Document doc, ElementId typeId, ElementId viewId, List<CurveLoop> loop, String name = "")
        {
            FilledRegion region = null;

            using (Transaction tran = new Transaction(doc, "Create Range Box"))
            {
                tran.Start();
                try
                {
                    region = FilledRegion.Create(doc, typeId, viewId, loop);
                    if (!String.IsNullOrEmpty(name))
                    {
                        region.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(name);
                    }
                    tran.Commit();
                }
                catch
                {
                    tran.RollBack();
                    throw;
                }
            }

            return region;
        }

        private static ElementId GetDefultFilledRegionTypeId(Document doc)
        {
            FilteredElementCollector filledRegionTypeCollector = new FilteredElementCollector(doc);
            filledRegionTypeCollector.OfClass(typeof(FilledRegionType));
            ElementId typeId = filledRegionTypeCollector.ToElements().FirstOrDefault(t => t.Name.Contains("透明"))?.Id ?? ElementId.InvalidElementId;

            return typeId;
        }
        #endregion

        #region DirectShape
        /// <summary>
        /// 生成常规模型
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transientSolid"></param>
        /// <param name="dsName"></param>
        /// <returns></returns>
        public static DirectShape CreateDirectShape(Document doc, Solid transientSolid, String dsName = "")
        {
            ElementId catId = new ElementId(BuiltInCategory.OST_GenericModel);  //常规模型
            DirectShape ds = DirectShape.CreateElement(doc, catId);

            if (ds.IsValidGeometry(transientSolid))
            {
                ds.SetShape(new GeometryObject[] { transientSolid });
            }
            else
            {
                var geoms = transientSolid.TessellateSolid(doc);
                ds.SetShape(geoms);
            }

            if (!String.IsNullOrEmpty(dsName))
                ds.Name = dsName;

            return ds;
        }
        #endregion

        /// <summary>
        /// 构造射线法查找对象
        /// </summary>
        /// <param name="currDocument">当前文件，如果你需要查找链接文件，这个参数也必须是uidoc.ActiveDocument</param>
        /// <param name="isFindInLinks">是否要查找连接文件</param>
        /// <param name="findReferenceTarget">需要查找的类型，可以是 Element Face Curve Edge Mesh 和 All </param>
        /// <param name="targetElementIds">目标元素的ElementId集合，如果你要查找链接文件中的元素，这个集合必须是RevitLinkInstance的Id，换句话说，集合中的元素Id不许都是当前文件中的Id</param>
        /// <param name="elementFilter">目标元素的过滤器</param>
        /// <returns></returns>
        public static ReferenceIntersector GetReferenceIntersector(Document currDocument, bool isFindInLinks, FindReferenceTarget findReferenceTarget, ICollection<ElementId> targetElementIds = null, ElementFilter elementFilter = null)
        {
            FilteredElementCollector collector = new FilteredElementCollector(currDocument);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(v3 => !(v3.IsTemplate));
            if (view3D == null) return null;

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3D)
            {
                FindReferencesInRevitLinks = isFindInLinks,
                TargetType = findReferenceTarget
            };
            if (targetElementIds != null && targetElementIds.Count > 0)
                referenceIntersector.SetTargetElementIds(targetElementIds);
            if (elementFilter != null)
                referenceIntersector.SetFilter(elementFilter);

            return referenceIntersector;
        }

        /// <summary>
        /// 通过查找到的ReferenceWithContext 获取对象
        /// </summary>
        /// <param name="referenceWithContext"></param>
        /// <param name="currDoc">当前稳点 Document</param>
        /// <returns></returns>
        public static Element GetElementByReferenceWithContext(this ReferenceWithContext referenceWithContext, Document currDoc)
        {
            if (referenceWithContext == null)
            {
                return null;
            }

            Reference reference = referenceWithContext.GetReference();
            Element element = null;
            if (reference.ElementId != ElementId.InvalidElementId)
            {//LinkedElement的ElementId是RevitLinkInstance
                element = currDoc.GetElement(reference.ElementId);
            }
            if (reference.LinkedElementId != ElementId.InvalidElementId)
            {
                element = LinkedElementUtils.GetLinkedDocumnet(currDoc, reference).GetElement(reference.LinkedElementId);
            }

            return element;
        }
    }
}
