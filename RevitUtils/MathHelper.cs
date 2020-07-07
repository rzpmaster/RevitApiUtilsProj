using System;

namespace RevitUtils
{
    public static class MathHelper
    {
        #region UnitConvert Related
        const double convertFeetToMm = 12 * 25.4;

        /// <summary>
        /// 毫米到英尺的转换系数
        /// </summary>
        public const double Mm2Feet = 1 / convertFeetToMm;

        /// <summary>
        /// 英尺到毫米的转换系数
        /// </summary>
        public const double Feet2Mm = convertFeetToMm;
        #endregion


        #region Compare Related
        const double tolerance = 1.0E-9;
        const double almostTolerance = 1E-2;

        public static double Eps
        {
            get
            {
                return tolerance;
            }
        }

        public static bool IsZero(this double a, double tolerance)
        {
            return tolerance > Math.Abs(a);
        }

        public static bool IsZero(this double a)
        {
            return IsZero(a, tolerance);
        }

        public static bool IsAlmostZero(this double a)
        {
            return IsZero(a, almostTolerance);
        }

        public static bool IsEqual(this double a, double b, double tolerance)
        {
            return IsZero(b - a, tolerance);
        }

        public static bool IsEqual(this double a, double b)
        {
            return IsEqual(a, b, tolerance);
        }

        public static bool IsAlmostEqual(this double a, double b)
        {
            return IsEqual(a, b, almostTolerance);
        }
        #endregion
    }
}
