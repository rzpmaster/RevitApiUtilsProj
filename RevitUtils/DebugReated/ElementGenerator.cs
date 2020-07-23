using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils.DebugReated
{
    public class ElementGenerator
    {
        #region DirectShape
        /// <summary>
        /// 生成给定元素的几何实体的常规模型，支持链接文件中的元素id
        /// </summary>
        /// <param name="document"></param>
        /// <param name="integerIds">元素id</param>
        /// <param name="needHighLight">是否需要高亮</param>
        public static List<ElementId> CreateDirectShapes(Document document, List<int> integerIds, bool needHighLight = true)
        {
            var docs = LinkedElementUtils.GetAllLinkedDocument(document);
            docs.Add(document);

            var ids = integerIds.Select(e => new ElementId(e)).ToList();

            var solids = new List<Solid>();
            foreach (var id in ids)
            {
                Element ele = null;
                foreach (var doc in docs)
                {
                    ele = doc.GetElement(id);
                    if (ele != null)
                        break;
                }

                if (ele == null) continue;
                if (ele is Room) solids.Add(RoomUtils.GetRoomActualSolid(ele as Room, document));
                else solids.Add(GeometryUtils.GetSolid(ele));
            }

            ids.Clear();
            using (Transaction tr = new Transaction(document, "DirectShapes"))
            {
                tr.Start();
                foreach (var item in solids)
                {
                    var directShapeId = CreateDirectShape(document, item);
                    ids.Add(directShapeId);
                }
                tr.Commit();
            }

            if (needHighLight) ViewUtils.HighLightElements(new UIDocument(document), ids);
            return ids;
        }

        /// <summary>
        /// 生成常规模型，记得开启事务！！！
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transientSolid"></param>
        /// <param name="isOpenTrans">是否在方法内部开启事务，如果为false，请手动在方法外部开启事务</param>
        /// <param name="dsName"></param>
        /// <returns></returns>
        /// <remarks>生成Solid不需要开启事务，但是DirectShape一定要开启事务</remarks>
        public static ElementId CreateDirectShape(Document doc, Solid transientSolid, bool isOpenTrans = false, String dsName = "")
        {
            ElementId catId = new ElementId(BuiltInCategory.OST_GenericModel);  //常规模型

            Transaction tr = null;
            if (isOpenTrans)
            {
                tr = new Transaction(doc, "DirectShape");
                tr.Start();
                //DirectShape ds = DirectShape.CreateElement(doc, catId);
                //SetShape(ds, transientSolid);
                //if (!String.IsNullOrEmpty(dsName))
                //    ds.Name = dsName;
            }

            // 需要开启事务
            DirectShape ds = DirectShape.CreateElement(doc, catId);
            SetShape(ds, transientSolid);
            if (!String.IsNullOrEmpty(dsName))
                ds.Name = dsName;

            tr?.Commit();
            tr?.Dispose();
            return ds.Id;
        }

        private static void SetShape(DirectShape ds, Solid transientSolid)
        {
            if (ds.IsValidGeometry(transientSolid))
            {
                ds.SetShape(new GeometryObject[] { transientSolid });
            }
            else
            {
                var geoms = transientSolid.TessellateSolid(ds.Document);
                ds.SetShape(geoms);
            }
        }
        #endregion

        #region FilledRegion
        /// <summary>
        /// 生成填充区域
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="typeId"></param>
        /// <param name="viewId"></param>
        /// <param name="loop"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ElementId CreateFilledRegion(Document doc, ElementId typeId, ElementId viewId, List<CurveLoop> loop, String name = "")
        {
            FilledRegion region = null;
            using (Transaction tran = new Transaction(doc, "FilledRegion"))
            {
                tran.Start();
                region = FilledRegion.Create(doc, typeId, viewId, loop);
                if (!String.IsNullOrEmpty(name))
                {
                    region.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(name);
                }
                tran.Commit();
            }
            return region.Id;
        }

        public static ElementId CreateFilledRegion(Document doc, List<CurveLoop> loop, String name = "")
        {
            var typeId = GetDefultFilledRegionTypeId(doc);
            return CreateFilledRegion(doc, typeId, new UIDocument(doc).ActiveView.Id, loop, name);
        }

        private static ElementId GetDefultFilledRegionTypeId(Document doc)
        {
            FilteredElementCollector filledRegionTypeCollector = new FilteredElementCollector(doc);
            filledRegionTypeCollector.OfClass(typeof(FilledRegionType));
            var typeId = filledRegionTypeCollector.ToElementIds().FirstOrDefault() ?? ElementId.InvalidElementId;

            foreach (var ele in filledRegionTypeCollector.ToElements())
            {
                if (ele.Name.Contains("对角线"))
                {
                    typeId = ele.Id;
                    break;
                }
            }

            return typeId;
        }
        #endregion

        /// <summary>
        /// 根据给定线，生成模型线
        /// </summary>
        /// <param name="document"></param>
        /// <param name="curves"></param>
        /// <param name="sketchPlane"></param>
        /// <param name="needHighLight"></param>
        /// <returns></returns>
        public static List<ElementId> CreateModelCurves(Document document, IEnumerable<Curve> curves, SketchPlane sketchPlane, bool needHighLight = false)
        {
            var ids = new List<ElementId>();
            using (var trans = new Transaction(document, "ModelCurves"))
            {
                trans.Start();
                foreach (var c in curves)
                {
                    var mc = document.Create.NewModelCurve(c, sketchPlane);
                    ids.Add(mc.Id);
                }
                trans.Commit();
            }
            if (needHighLight) ViewUtils.HighLightElements(new UIDocument(document), ids);
            return ids;
        }

        /// <summary>
        /// 根据给定线，生成详图线
        /// </summary>
        /// <param name="document"></param>
        /// <param name="curves"></param>
        /// <param name="view"></param>
        /// <param name="needHighLight"></param>
        /// <returns></returns>
        public static List<ElementId> CreateDetailCurves(Document document, IEnumerable<Curve> curves, View view, bool needHighLight = false)
        {
            var ids = new List<ElementId>();
            using (var trans = new Transaction(document, "DetailCurves"))
            {
                trans.Start();
                foreach (var c in curves)
                {
                    var mc = document.Create.NewDetailCurve(view, c);
                    ids.Add(mc.Id);
                }
                trans.Commit();
            }
            if (needHighLight) ViewUtils.HighLightElements(new UIDocument(document), ids);
            return ids;
        }
    }
}
