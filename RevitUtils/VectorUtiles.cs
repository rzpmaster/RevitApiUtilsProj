using Autodesk.Revit.DB;
using System;

namespace RevitUtils
{
    public static class VectorUtiles
    {
        /// <summary>
        /// 判断向量是否垂直
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static bool IsVerticalTo(this XYZ vector1, XYZ vector2)
        {
            return vector1.DotProduct(vector2).IsZero();

            //return Math.Abs((vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z)) <= tolerance ? true : false; //数据精度问题~
        }

        /// <summary>
        /// 判断向量是否垂直
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="tolerance">点乘等于0的tolerance</param>
        /// <returns></returns>
        public static bool IsVerticalTo(this XYZ vector1, XYZ vector2, double tolerance)
        {
            return vector1.DotProduct(vector2).IsZero(tolerance);
        }

        /// <summary>
        /// 判断向量是否近似垂直
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        /// <remarks>点乘约等于0，默认tolerance为0.01</remarks>
        public static bool IsAlmostVerticalTo(this XYZ vector1, XYZ vector2)
        {
            return vector1.DotProduct(vector2).IsAlmostZero();
        }

        /// <summary>
        /// 判断向量是否平行
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this XYZ vector1, XYZ vector2)
        {
            return vector1.CrossProduct(vector2).GetLength().IsZero();

            //bool tag = Math.Abs(vector1.X * vector2.Y - vector1.Y * vector2.X) < tolerance &&
            //        Math.Abs(vector1.Y * vector2.Z - vector1.Z * vector2.Y) < tolerance &&
            //        Math.Abs(vector1.X * vector2.Z - vector1.Z * vector2.X) < tolerance;
            //return tag;
        }

        /// <summary>
        /// 判断向量是否平行
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="tolerance">叉乘长度为0的tolerance</param>
        /// <returns></returns>
        public static bool IsParallelTo(this XYZ vector1, XYZ vector2, double tolerance)
        {
            return vector1.CrossProduct(vector2).GetLength().IsZero(tolerance);
        }

        /// <summary>
        /// 判断向量是否近似平行
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        /// <remarks>叉乘长度约等于0，默认tolerance为0.01</remarks>
        public static bool IsAlmostParallelTo(this XYZ vector1, XYZ vector2)
        {
            return vector1.CrossProduct(vector2).GetLength().IsAlmostZero();
        }

        /// <summary>
        /// 判断向量是否近似平行
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="tolerance">两向量的角度tolerance，默认为1°（角度制）</param>
        /// <returns></returns>
        /// <remarks>向量角度在±tolerance之间</remarks>
        public static bool IsAlmostParallelToByAngle(this XYZ vector1, XYZ vector2, double tolerance = 1)
        {
            double angle = vector1.AngleTo(vector2) * 180 / Math.PI;
            return angle.IsZero(tolerance) || (180 - angle).IsZero(tolerance);
        }

        /// <summary>
        /// 是否水平向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static bool IsHorizontal(this XYZ vector)
        {
            return vector.Z.IsZero();
        }

        /// <summary>
        /// 是否竖直向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static bool IsVertical(this XYZ vector)
        {
            return vector.X.IsZero() && vector.Y.IsZero();
        }

        /// <summary>
        /// 获得一个向量的 任意法向量 单位向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [Obsolete("有风险", false)]
        public static XYZ GetNormal(this XYZ vector)
        {
            return new XYZ(vector.Y + vector.Z, -vector.X + vector.Z, -vector.X - vector.Y).Normalize();
        }

        /// <summary>
        /// 获得一个向量在xoy平面内的任一法向量 单位向量。如果向量是竖直的，返回 x轴向 单位向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [Obsolete("有风险", false)]
        public static XYZ GetNormalInXoy2(this XYZ vector)
        {
            return vector.GetNormal().ProjectXoy().Normalize();
        }

        /// <summary>
        /// 获得一个向量在xoy平面内的任一法向量 单位向量。如果向量是竖直的，返回 x轴向 单位向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static XYZ GetNormalInXoy(this XYZ vector)
        {
            if (vector.IsVertical()) return XYZ.BasisX;

            return vector.CrossProduct(XYZ.BasisZ).Normalize();
        }

        #region VectorMatrix
        /// <summary>
		/// multiplication cross of two Autodesk.Revit.DB.XYZ as Matrix
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static XYZ CrossMatrix(XYZ p1, XYZ p2)
        {
            double v1 = p1.X;
            double v2 = p1.Y;
            double v3 = p1.Z;

            double u1 = p2.X;
            double u2 = p2.Y;
            double u3 = p2.Z;

            double x = v3 * u2 - v2 * u3;
            double y = -v3 * u1 + v1 * u3;
            double z = v2 * u1 - v1 * u2;

            XYZ point = new XYZ(x, y, z);
            return point;
        }

        /// <summary>
        /// dot product of two Autodesk.Revit.DB.XYZ as Matrix
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double DotMatrix(XYZ p1, XYZ p2)
        {
            double v1 = p1.X;
            double v2 = p1.Y;
            double v3 = p1.Z;

            double u1 = p2.X;
            double u2 = p2.Y;
            double u3 = p2.Z;

            double result = v1 * u1 + v2 * u2 + v3 * u3;

            return result;
        }
        #endregion
    }
}
