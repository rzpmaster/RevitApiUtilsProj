using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RevitUtils
{
    // HostObjectUtils 类中可以获得宿主元素的顶面 底面 和侧面。

    /// <summary>
    /// Face Class is Inherited from GeometryObject
    /// and its subclass has
    /// PlanarFace          平面
    /// ConicalFace         圆锥面
    /// CylindricalFace     圆柱面
    /// RevolvedFace        通过绕一个轴旋转得到的面
    /// RuledFace           扫掠得到的面
    /// HermiteFace         通过Hermite插值定义的面
    /// </summary>
    public static class FaceUtils
    {
        /// <summary>
        /// 获取面的法向量
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static XYZ FaceNormal(this Face face)
        {
            var bbox = face.GetBoundingBox();
            return face.ComputeNormal(bbox.Min);
        }

        /// <summary>
        /// 获取面三角化后的顶点集合
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static IList<XYZ> GetPoints(this Face face)
        {
            List<XYZ> points = new List<XYZ>();
            return face.Triangulate().Vertices;
        }

        /// <summary>
        /// 判断面是否是水平面
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static bool IsHorizontalFace(Face face)
        {
            var points = GetPoints(face);
            double z1 = points[0].Z;
            double z2 = points[1].Z;
            double z3 = points[2].Z;
            double z4 = points[3].Z;
            bool flag = MathHelper.IsEqual(z1, z2);
            flag = flag && MathHelper.IsEqual(z2, z3);
            flag = flag && MathHelper.IsEqual(z3, z4);
            flag = flag && MathHelper.IsEqual(z4, z1);

            return flag;
        }

        /// <summary>
        /// 判断面是否和直线平行
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this Face face, Line line)
        {
            var points = GetPoints(face);
            XYZ vector1 = points[0] - points[1];
            XYZ vector2 = points[1] - points[2];

            XYZ cross = vector1.CrossProduct(vector2);
            return cross.IsVerticalTo(line.Direction);
        }

        /// <summary>
        /// 判断面是否和直线垂直
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsVerticalTo(this Face face, Line line)
        {
            var points = GetPoints(face);
            XYZ vector1 = points[0] - points[1];
            XYZ vector2 = points[1] - points[2];

            XYZ cross = vector1.CrossProduct(vector2);
            return cross.IsParallelTo(line.Direction);
        }

        /// <summary>
        /// 获取Solid法向量为normal的一个面
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Face GetFaceByNormal(Solid solid, XYZ normal)
        {
            Face face = null;
            Face almostFace = null;

            var faceArray = solid.Faces;
            foreach (Face f in faceArray)
            {
                var nor = FaceNormal(f);
                if (nor.IsAlmostEqualTo(normal))
                {
                    face = f;
                }
                if (nor.AngleTo(normal) < (Math.PI / 2))
                {
                    almostFace = f;
                }
            }

            var relt = face != null ? face : almostFace;

            return relt == null ? null : relt;
        }

        /// <summary>
        /// 获取Solid法向量为normal的所有面
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="normal">如果为空,则返回所有面</param>
        /// <returns></returns>
        public static IEnumerable<Face> GetFacesByNormal(Solid solid, XYZ normal = null)
        {
            foreach (Face f in solid.Faces)
            {
                if (normal == null)
                {
                    yield return f;
                }
                else if (f.FaceNormal().IsAlmostEqualTo(normal) ||
                         f.FaceNormal().IsAlmostEqualTo(normal.Negate()))
                {
                    yield return f;
                }
            }

            yield break;
        }

        /// <summary>
        /// 获取Solid的底面
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Face GetBottomFace(Solid solid)
        {
            return GetFaceByNormal(solid, XYZ.BasisZ.Negate());
        }

        /// <summary>
        /// 获取Solid的顶面
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Face GetTopFace(Solid solid)
        {
            return GetFaceByNormal(solid, XYZ.BasisZ);
        }

        /// <summary>
        /// 获取Element法向量为normal的一个面
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Face GetElementFaceByDirection(Element elem, XYZ normal)
        {
            var solid = GeometryUtils.GetSolid(elem);
            return GetFaceByNormal(solid, normal);
        }

        /// <summary>
        /// 获取Element法向量为normal的所有面
        /// </summary>
        /// <param name="element"></param>
        /// <param name="normal">normal为空时,返回所有面</param>
        /// <returns></returns>
        public static IEnumerable<Face> GetFacesByNormal(Element element, XYZ normal = null)
        {
            var solids = GeometryUtils.GetSolids(element);
            foreach (var solid in solids)
            {
                foreach (var face in GetFacesByNormal(solid, normal))
                {
                    yield return face;
                }
            }

            yield break;
        }

        /// <summary>
        /// 获取Elment的底面
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public static Face GetElementBottomFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ.Negate());
        }

        /// <summary>
        /// 获取Element的顶面
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public static Face GetElementTopFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ);
        }
    }
}
