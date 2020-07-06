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
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="builtInCategory"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<Element> GetElementsBySolid(this Document document, Solid solid, BuiltInCategory builtInCategory, bool inverted = false)
        {
            var collector = new FilteredElementCollector(document);
            var solidFilter = new ElementIntersectsSolidFilter(solid, inverted);
            var list = collector.OfCategory(builtInCategory)
                                .WhereElementIsNotElementType()
                                .WherePasses(solidFilter).ToElements();
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
        public static FilledRegion CreateRangeBoxByFilledRegion(this Document doc, List<CurveLoop> loop, String name = "")
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
        public static FilledRegion CreateRangeBoxByFilledRegion(this Document doc, ElementId typeId, ElementId viewId, List<CurveLoop> loop, String name = "")
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
        public static DirectShape CreateDirectShape(this Document doc, Solid transientSolid, String dsName = "")
        {
            ElementId catId = new ElementId(BuiltInCategory.OST_GenericModel);  //常规模型
            DirectShape ds = DirectShape.CreateElement(doc, catId);

            if (ds.IsValidGeometry(transientSolid))
            {
                ds.SetShape(new GeometryObject[] { transientSolid });
            }
            else
            {
                TessellatedShapeBuilderResult result = GetTessellatedSolid(doc, transientSolid);
                ds.SetShape(result.GetGeometricalObjects());
            }

            if (!String.IsNullOrEmpty(dsName))
                ds.Name = dsName;

            return ds;
        }

        private static TessellatedShapeBuilderResult GetTessellatedSolid(Document doc, Solid transientSolid)
        {
            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            ElementId idMaterial = new FilteredElementCollector(doc)
                                        .OfClass(typeof(Material))
                                        .FirstElementId();

            ElementId idGraphicsStyle = new FilteredElementCollector(doc).
                                            OfClass(typeof(GraphicsStyle)).
                                            ToElementIds().First();

            builder.OpenConnectedFaceSet(true);

            FaceArray faceArray = transientSolid.Faces;

            foreach (Face face in faceArray)
            {
                List<XYZ> triFace = new List<XYZ>(3);
                Mesh mesh = face.Triangulate();

                if (null == mesh)
                    continue;

                int triCount = mesh.NumTriangles;

                for (int i = 0; i < triCount; i++)
                {
                    triFace.Clear();

                    for (int n = 0; n < 3; n++)
                    {
                        triFace.Add(mesh.get_Triangle(i).get_Vertex(n));
                    }

                    builder.AddFace(new TessellatedFace(triFace, idMaterial));
                }
            }

            builder.CloseConnectedFaceSet();

            // return builder.Build(TessellatedShapeBuilderTarget.Solid, TessellatedShapeBuilderFallback.Abort, idGraphicsStyle);
            return builder.GetBuildResult();
        }
        #endregion
    }
}
