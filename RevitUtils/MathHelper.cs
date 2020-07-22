using Autodesk.Revit.DB;
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

        /// <summary>
        /// 米到英尺的转换系数
        /// </summary>
        public const double M2Feet = 1000 * Mm2Feet;

        /// <summary>
        /// 英尺到米的转换系数
        /// </summary>
        public const double Feet2M = Feet2Mm / 1000;

        /// <summary>
        /// Feet转MM
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FeetToMM(this double value)
        {
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// Feet转M
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FeetToM(this double value)
        {
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_METERS);
        }

        /// <summary>
        /// MM转Feet
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MMToFeet(this double value)
        {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// M转Feet
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double MToFeet(this double value)
        {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_METERS);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToRad(this double value)
        {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_DECIMAL_DEGREES);
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDegree(this double value)
        {
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_DECIMAL_DEGREES);
        }
        #endregion

        #region Compare Related
        const double tolerance = 1.0E-6;
        const double almostTolerance = 1E-2;

        const double rvtVertexTolerance = 0.0005233832795;              //5.23E-4    0.16mm
        const double rvtAngleTolerance = 0.0017453292519943279;         //1.74E-3    0.1°
        const double rvtShortCurveTolerance = 0.0025602645572916664;    //2.56E-3    0.78mm

        public static double Eps
        {
            get
            {
                return tolerance;
            }
        }

        public static double VertexTolerance
        {
            get
            {
                return rvtVertexTolerance;
            }
        }

        public static double AngleTolerance
        {
            get
            {
                return rvtAngleTolerance;
            }
        }

        public static double ShortCurveTolerance
        {
            get
            {
                return rvtShortCurveTolerance;
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

        public static bool IsGreaterThan(this double d1, double d2)
        {
            //要绝对大于
            if (d1 > d2 && !d1.IsEqual(d2))
            {
                return true;
            }
            return false;
        }

        public static bool IsGreaterThanOrEqual(this double d1, double d2)
        {
            if (d1 > d2 || d1.IsEqual(d2))
            {
                return true;
            }
            return false;
        }

        public static bool IsLessThan(this double d1, double d2)
        {
            //要绝对大于
            if (d1 < d2 && !d1.IsEqual(d2))
            {
                return true;
            }
            return false;
        }

        public static bool IsLessThanOrEqual(this double d1, double d2)
        {
            if (d1 < d2 || d1.IsEqual(d2))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
