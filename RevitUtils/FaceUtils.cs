using Autodesk.Revit.DB;
using System;

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
        /// <param name="f"></param>
        /// <returns></returns>
        public static XYZ FaceNormal(this Face f)
        {
            var bbox = f.GetBoundingBox();

            return f.ComputeNormal(bbox.Min);
        }

        /// <summary>
        /// 获取Solid的底面
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Face GetBottomFace(Solid solid)
        {
            return GetSoildFaceByDirection(solid, XYZ.BasisZ.Negate());
        }

        /// <summary>
        /// 获取Solid的顶面
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Face GetTopFace(Solid solid)
        {
            return GetSoildFaceByDirection(solid, XYZ.BasisZ);
        }

        /// <summary>
        /// 返回以给定方向为法线方向的面
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Face GetSoildFaceByDirection(Solid solid, XYZ direction)
        {
            Face face = null;
            Face almostFace = null;

            var faceArray = solid.Faces;
            foreach (Face f in faceArray)
            {
                var nor = FaceNormal(f);
                if (nor.IsAlmostEqualTo(direction))
                {
                    face = f;
                }
                if (nor.AngleTo(direction) < (Math.PI / 2))
                {
                    almostFace = f;
                }
            }

            var relt = face != null ? face : almostFace;

            return relt == null ? null : relt;
        }

        /// <summary>
        /// 返回以给定方向为法线方向的面
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Face GetElementFaceByDirection(Element elem, XYZ direction)
        {
            var solid = GeometryUtils.GetSolid(elem);
            return GetSoildFaceByDirection(solid, direction);
        }

        public static Face GetElementBottomFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ.Negate());
        }

        public static Face GetElementTopFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ);
        }
    }
}
