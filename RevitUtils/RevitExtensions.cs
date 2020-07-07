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
        /// 从给定点发出一条射线,返回第一个碰到的ReferenceWithContext
        /// </summary>
        /// <param name="document"></param>
        /// <param name="point"></param>
        /// <param name="rayDirection"></param>
        /// <param name="targetCategories"></param>
        /// <returns></returns>
        public static ReferenceWithContext GetReferenceByRay(Document document, XYZ point, XYZ rayDirection, params BuiltInCategory[] targetCategories)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));

            // 目标元素类型或过滤器
            List<ElementFilter> filters = new List<ElementFilter>();
            foreach (var category in targetCategories)
            {
                ElementFilter filter = new ElementCategoryFilter(category);
                filters.Add(filter);
            }
            LogicalOrFilter orFilter = new LogicalOrFilter(filters);

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(orFilter, FindReferenceTarget.Element, view3D);
            referenceIntersector.FindReferencesInRevitLinks = document.IsLinked;
            var referenceWithContext = referenceIntersector.FindNearest(point, rayDirection);
            if (referenceWithContext != null)
            {
                if ((referenceWithContext.Proximity > MathHelper.Eps) &&
                    (referenceWithContext.Proximity < 50 * 1000 * MathHelper.Mm2Feet)) //大于50m就算获取失败
                {
                    return referenceWithContext;
                }
            }

            return null;
        }
    }
}
