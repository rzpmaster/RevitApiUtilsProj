using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    /// <summary>
    /// GeometryObject's subclass has
    /// 
    /// GeometryElement     !!!可迭代
    /// GeometryInstance    !!!包括 实例几何 和 类型几何
    /// Solid
    /// Face
    /// Curve Edges
    /// Point
    /// Mesh
    /// 
    /// PolyLine 多段线 
    /// Profile 轮廓线 （可以填充）
    /// </summary>
    public static class GeometryUtils
    {
        /// <summary>
        /// 获得元素的一个Solid
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <remarks>注意，这里只是返回一个Solid，返回的Solid有可能不能代表这个元素的Geometry。因为有的元素有都有很多Solid，尤其是嵌套族，当需要获取到实例的全部几何形状，应该使用下面的GetSolids()方法</remarks>
        public static Solid GetSolid(this Element e)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;

            GeometryElement geo = e.get_Geometry(options);

            // 有些Element没有几何实体，必须从族类型中获取
            var solid = GetGeometryElementSolid(geo);

            return solid;
        }

        public static Solid GetGeometryElementSolid(GeometryElement geoElem)
        {
            Solid solid = null;

            foreach (GeometryObject obj in geoElem)
            {
                if (obj is Solid)
                {
                    solid = obj as Solid;
                    if (null != solid && 0 < solid.Faces.Size)
                    {
                        break;
                    }
                }
                else if (obj is GeometryInstance)
                {
                    GeometryElement instGeo = (obj as GeometryInstance).GetInstanceGeometry();
                    solid = GetGeometryElementSolid(instGeo);
                    if (null != solid && 0 < solid.Faces.Size)
                    {
                        break;
                    }
                    else
                    {
                        GeometryElement syGeo = (obj as GeometryInstance).GetSymbolGeometry();
                        solid = GetGeometryElementSolid(syGeo);
                    }
                }
            }

            return solid;
        }

        /// <summary>
        /// 获得元素实例的所有Solid
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<Solid> GetSolids(this Element element)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;

            GeometryElement geomElem = element.get_Geometry(options);
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj == null)
                {
                    continue;
                }

                if (geomObj is Solid)
                {
                    Solid solid = geomObj as Solid;
                    if (solid != null && solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        yield return solid;
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = geomObj as GeometryInstance;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj == null)
                        {
                            continue;
                        }

                        if (instGeomObj is Solid)
                        {
                            Solid solid = instGeomObj as Solid;
                            if (solid != null && solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                yield return solid;
                            }
                        }
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// 三角化Solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transientSolid"></param>
        /// <returns></returns>
        public static IList<GeometryObject> TessellateSolid(this Solid transientSolid, Document doc)
        {
            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            ElementId idMaterial = new FilteredElementCollector(doc)
                                        .OfClass(typeof(Material))
                                        .FirstElementId();

            ElementId idGraphicsStyle = new FilteredElementCollector(doc)
                                            .OfClass(typeof(GraphicsStyle))
                                            .FirstElementId();

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
            return builder.GetBuildResult().GetGeometricalObjects();
        }

        /// SolidUtils 中查看更多Solid工具

        /// GeometryCreationUtilities 中查看更过构造Solid的方法：
        /// 拉伸Extrusion 融合Blend 旋转Revolved 放样Swept 放样融合SweptBlend

        /// <summary>
        /// 判断一个点是否在多边形内部(xoy平面内，不考虑高度)
        /// http://alienryderflex.com/polygon/
        /// https://github.com/wieslawsoltes/Math.Spatial/blob/master/src/Math.Spatial/Polygon2.cs
        /// </summary>
        /// <param name="polygon">多边形顶点集合</param>
        /// <param name="point">要判断的点</param>
        /// <returns></returns>
        /// <remarks>不安全，没有判断边界条件</remarks>
        public static bool IsPointInPolygon(IList<XYZ> polygon, UV point)
        {
            bool contains = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.V) != (polygon[j].Y > point.V))
                    && (point.U < (((polygon[j].X - polygon[i].X) * (point.V - polygon[i].Y)) / (polygon[j].Y - polygon[i].Y)) + polygon[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        public static bool IsPointInPolygon(IList<XYZ> polygon, XYZ point)
        {
            return IsPointInPolygon(polygon, point.ToUV());
        }

        public static bool IsPointInPolygon(this CurveLoop loop, XYZ point)
        {
            var polygon = loop.ToPointsList();
            return IsPointInPolygon(polygon, point);
        }

        public static bool IsPointInPolygon(this IList<CurveLoop> loops, XYZ point)
        {
            bool isInPolygon = false;
            foreach (CurveLoop loop in loops)
            {
                if (IsPointInPolygon(loop, point)) isInPolygon = !isInPolygon;
            }
            return isInPolygon;
        }

        /// <summary>
        /// 判断线是否在多边形内(xoy平面内，不考虑高度)
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsCurveInPolygon(this CurveLoop loop, Curve curve)
        {
            return IsPointInPolygon(loop, curve.GetEndPoint(0)) &&
                   IsPointInPolygon(loop, curve.GetEndPoint(1));
        }

        public static bool IsCurveInPolygon(this IList<CurveLoop> loops, Curve curve)
        {
            return IsPointInPolygon(loops, curve.GetEndPoint(0)) &&
                   IsPointInPolygon(loops, curve.GetEndPoint(1));
        }

        /// <summary>
        /// 判断线是否与多边形相交(xoy平面内，不考虑高度)
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static bool IsCurveIntersectPolygon(this CurveLoop loop, Curve curve)
        {
            return IsPointInPolygon(loop, curve.GetEndPoint(0)) ^
                   IsPointInPolygon(loop, curve.GetEndPoint(1));
        }

        public static bool IsCurveIntersectPolygon(this IList<CurveLoop> loops, Curve curve)
        {
            return IsPointInPolygon(loops, curve.GetEndPoint(0)) ^
                   IsPointInPolygon(loops, curve.GetEndPoint(1));
        }
    }
}
