using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace RevitUtils
{
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
        /// 判断一个点是否在多边形内部（xoy平面内）
        /// http://alienryderflex.com/polygon/
        /// https://github.com/wieslawsoltes/Math.Spatial/blob/master/src/Math.Spatial/Polygon2.cs
        /// </summary>
        /// <param name="polygon">多边形顶点集合</param>
        /// <param name="point">要判断的点</param>
        /// <returns></returns>
        /// <remarks>不安全，没有判断边界条件</remarks>
        public static bool IsPointInPolygon(List<XYZ> polygon, XYZ point)
        {
            bool contains = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y))
                    && (point.X < (((polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y)) / (polygon[j].Y - polygon[i].Y)) + polygon[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }
    }
}
