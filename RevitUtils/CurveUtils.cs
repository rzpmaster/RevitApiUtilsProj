using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    /// <summary>
    /// Curve Class is Inherited from GeometryObject
    /// and its subclass has
    /// Line                直线
    /// Arc                 圆弧
    /// Ellipse             椭圆弧
    /// CylindricalHelix    螺旋线
    /// HermiteSpline       H样条曲线
    /// NurbSpline          N样条曲线
    /// </summary>
    public static class CurveUtils
    {
        #region Curve Related
        /// <summary>
        /// 判断两条曲线是否垂直
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsVerticalTo(this Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsVerticalTo(xyz2);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否垂直
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsVerticalTo(this Curve curve1, Curve curve2, double tolerance)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsVerticalTo(xyz2, tolerance);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否近似垂直
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsAlmostVerticalTo(this Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsAlmostVerticalTo(xyz2);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否平行
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsParallelTo(xyz2);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否平行
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsParallelTo(this Curve curve1, Curve curve2, double tolerance)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsParallelTo(xyz2, tolerance);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否近似平行
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool IsAlmostParallelTo(this Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsAlmostParallelTo(xyz2);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否近似平行
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <param name="tolerance">两向量的角度容忍值，默认为1°（角度制）</param>
        /// <returns></returns>
        /// <remarks>向量角度在±tolerance之间</remarks>
        public static bool IsAlmostParallelTo(this Curve curve1, Curve curve2, double tolerance)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return xyz1.IsAlmostParallelToByAngle(xyz2, tolerance);
            }
            return false;
        }

        /// <summary>
        /// 计算平行线之间的距离
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns>如果平行,返回他们之间的距离;否则,返回NaN</returns>
        public static double DistanceTo(this Curve curve1, Curve curve2)
        {
            if (!curve1.IsAlmostParallelTo(curve2))
            {
                return double.NaN;
            }

            Curve temp = curve1.Clone();
            temp.MakeUnbound();
            return temp.Distance(curve2.Evaluate(0.5, true));
        }

        /// <summary>
        /// 获得给定线给定点一侧的法向量 单位向量
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point">不在给定Curve上的任意点</param>
        /// <param name="normalizedParameter"></param>
        /// <returns></returns>
        public static XYZ GetNormalInXoy(this Curve curve, XYZ point, double normalizedParameter = 0.5)
        {
            var normal = curve.ComputeDerivatives(normalizedParameter, true).BasisX.GetNormalInXoy();

            XYZ start = curve.GetEndPoint(0);
            XYZ tempVector = new XYZ(point.X, point.Y, start.Z) - start;     //指向p点，并忽略高度
            double dotproduct = tempVector.DotProduct(normal);

            //等于0说明点在线上，任然返回正向法线
            return dotproduct >= 0 ? normal : normal.Negate();
        }

        /// <summary>
        /// 将Curve投影到XOY平面
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <remarks>
        /// 由于除了直线外,其他线的投影形状不确定,所以目前只支持直线投影
        /// </remarks>
        public static Curve ProjectXoy(this Curve curve)
        {
            Curve nc = null;
            if (curve is Line)
            {//直线
                if (!curve.IsBound)
                {
                    var op = (curve as Line).Origin.ProjectXoy();
                    var d = (curve as Line).Direction.ProjectXoy();

                    nc = Line.CreateUnbound(op, d);
                }
                else
                {
                    var sp = curve.GetEndPoint(0).ProjectXoy();
                    var ep = curve.GetEndPoint(1).ProjectXoy();

                    nc = Line.CreateBound(sp, ep);
                }
            }

            //TODO:除直线外，其他线的投影形状不确定，无法简单构造，暂留待以后补充

            else if (curve is Arc)
            {//圆弧

            }
            else if (curve is Ellipse)
            {//椭圆弧

            }
            else if (curve is CylindricalHelix)
            {//螺旋线

            }
            else if (curve is HermiteSpline)
            {//H样条曲线

            }
            else if (curve is NurbSpline)
            {//N样条曲线

            }

            //暂时采取将平行于xoy平面的线做平移处理，不平行与不处理，返回空
            if (nc != null && !(curve is CylindricalHelix) &&
                curve.ComputeDerivatives(0, true).BasisZ.IsVertical() &&
                curve.ComputeDerivatives(0.5, true).BasisZ.IsVertical() &&
                curve.ComputeDerivatives(1, true).BasisZ.IsVertical()
                )
            {
                XYZ vector = XYZ.Zero - new XYZ(0, 0, curve.GetEndPoint(0).Z);
                Transform transform = Transform.CreateTranslation(vector);
                nc = curve.CreateTransformed(transform);
            }

            return nc;
        }

        #endregion

        #region CurveLoop Related
        /// <summary>
        /// 获得 CurveLoop 的所有点
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static IList<XYZ> ToPointsList(this CurveLoop loop)
        {
            int n = loop.Count();
            List<XYZ> polygon = new List<XYZ>();

            foreach (Curve e in loop)
            {
                IList<XYZ> pts = e.Tessellate();

                n = polygon.Count;

                if (0 < n)
                {
                    //Debug.Assert(pts[0].IsAlmostEqualTo(polygon[n - 1]),
                    //  "expected last edge end point to equal next edge start point");

                    polygon.RemoveAt(n - 1);
                }
                polygon.AddRange(pts);
            }
            n = polygon.Count;

            //Debug.Assert(polygon[0].IsAlmostEqualTo(polygon[n - 1]),
            //  "expected first edge start point to equal last edge end point");

            polygon.RemoveAt(n - 1);

            return polygon;
        }

        /// <summary>
        /// 将一组点构造成一个首位相连的由直线组成的 CurveLoop
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static CurveLoop ToCurveLoop(this IList<XYZ> points)
        {
            List<Curve> curves = new List<Curve>();
            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                Line line = Line.CreateBound(points[j], points[i]);
                curves.Add(line);
            }

            return CurveLoop.Create(curves);
        }

        /// <summary>
        /// 将CurveLoop压平在xoy平面
        /// </summary>
        /// <param name="loop"></param>
        /// <returns>如果loop不能确定一个平面，则返回空</returns>
        public static CurveLoop ProjectXoy(this CurveLoop loop)
        {
            Plane plane = null;
            try
            {
                plane = loop.GetPlane();
            }
            catch { /*loop没有在平面内*/ }

            if (plane != null && !plane.Normal.IsVertical())
                return null;

            XYZ vector = XYZ.Zero - new XYZ(0, 0, loop.FirstOrDefault().GetEndPoint(0).Z);
            Transform transform = Transform.CreateTranslation(vector);
            return CurveLoop.CreateViaTransform(loop, transform);
        }

        /// <summary>
        /// 计算面积
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static double ComputeArea(this CurveLoop loop)
        {
            return ExporterIFCUtils.ComputeAreaOfCurveLoops(new List<CurveLoop> { loop });
        }

        /// <summary>
        /// 计算多边形的面积
        /// </summary>
        /// <param name="loops"></param>
        /// <returns></returns>
        public static double ComputeArea(this IList<CurveLoop> loops)
        {
            return ExporterIFCUtils.ComputeAreaOfCurveLoops(loops);
        }

        /// <summary>
        /// 对一组曲线循环进行排序，以使外部循环和内部循环分开
        /// more info :https://thebuildingcoder.typepad.com/blog/2015/01/exporterifcutils-curve-loop-sort-and-validate.html
        /// </summary>
        /// <param name="loops"></param>
        /// <returns>循环的排序集合</returns>
        /// <remarks>
        /// 外层循环是分开的，内层循环是根据它们的外层循环分组的。假定循环是不相交的，并且内部循环不会嵌套，即，内部循环的内部循环是另一个外部循环。
        /// </remarks>
        public static IList<IList<CurveLoop>> SortCurveLoops(this IList<CurveLoop> loops)
        {
            return ExporterIFCUtils.SortCurveLoops(loops);
        }

        /// <summary>
        /// 对曲线循环列表执行有效性检查，以确保它们全部共面，闭合且方向正确
        /// more info  :https://thebuildingcoder.typepad.com/blog/2015/01/exporterifcutils-curve-loop-sort-and-validate.html
        /// </summary>
        /// <param name="curveLoops">要检查的循环</param>
        /// <param name="extrDirVec">法向量</param>
        /// <returns>如果可能，返回正确定向的曲线环。如果不是，则返回不包含循环。</returns>
        public static IList<CurveLoop> ValidateCurveLoops(this IList<CurveLoop> curveLoops, XYZ extrDirVec)
        {
            return ExporterIFCUtils.ValidateCurveLoops(curveLoops, extrDirVec);
        }

        /// <summary>
        /// 检查CurveLoop有效性
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="extrDirVec">法向量</param>
        /// <returns></returns>
        public static CurveLoop ValidateCurveLoop(this CurveLoop loop, XYZ extrDirVec)
        {
            return ExporterIFCUtils.ValidateCurveLoops(new List<CurveLoop>() { loop }, extrDirVec).FirstOrDefault();
        }

        #region ValidateCurveLoop Spare Method
        /// <summary>
        /// 检查是否首位相连
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="tolerance"></param>
        /// <returns>返回首位相连的Loop</returns>
        /// <remarks>
        /// 顺序遍历前后连条直线,如果不首位相连,则
        /// 1.如果他们不平行,延长他们使之相交
        /// 2.如果平行,检查距离是否大于给定tolerance:如果大于,在他们之间添加直线使之连接;如果小于,则平移后一条直线使之在前面一条直线的延长线上,在用直线相连
        /// </remarks>
        public static CurveLoop ValidateCurveLoop(this CurveLoop loop, double tolerance = 300 * MathHelper.Mm2Feet)
        {
            List<Curve> lines = new List<Curve>();

            var enumerator = loop.GetEnumerator();
            Queue<Curve> queue = new Queue<Curve>(2);
            while (enumerator.MoveNext())
            {
                Curve currCurve = enumerator.Current;
                queue.Enqueue(currCurve);

                if (queue.Count != 2)
                {
                    continue;
                }

                Curve preCurve = queue.Dequeue();
                //处理当前两条直线
                DealWithCurrTwoCurvres(ref currCurve, ref preCurve, out Line tempLine, tolerance);

                lines.Add(preCurve);
                if (tempLine != null) lines.Add(tempLine);

                //更新一个在队列中的Curve
                queue.Dequeue();
                queue.Enqueue(currCurve);
            }

            #region while循环之后 需要判断第一条和最后一条直线的关系
            Curve lastCurve = queue.Dequeue();
            Curve firstCurve = lines.First();
            bool isMoved2 = DealWithCurrTwoCurvres(ref firstCurve, ref lastCurve, out Line tempLine2, tolerance);

            lines.Add(lastCurve);
            if (tempLine2 != null) lines.Add(tempLine2);
            //把firstCurve替换了
            lines.RemoveAt(0);
            lines.Insert(0, firstCurve);

            //如果移动过currCurve(firstCurve)
            if (isMoved2)
            {
                List<Curve> parallelFirstCurves = new List<Curve>();    //第一条线 后面(相邻的) 和第一条线平行的线
                Curve notParallelFirstCurve = null;                     //第一条线 后面 第一个和他不平行的线
                for (int i = 1; i < lines.Count; i++)
                {
                    var tempLine = lines[i--];
                    lines.Remove(tempLine);

                    if (CurveUtils.IsAlmostParallelTo(firstCurve, tempLine))
                    {
                        parallelFirstCurves.Add(tempLine);
                        continue;
                    }
                    else
                    {
                        notParallelFirstCurve = tempLine;
                        break;
                    }
                }

                if (parallelFirstCurves.Count == 0)
                {
                    ExtendTwoCurves(ref notParallelFirstCurve, ref firstCurve);
                    lines.Insert(1, notParallelFirstCurve);
                }
                else
                {//先把所有和第一条直线平行的线，平移一定距离
                    double distance = firstCurve.DistanceTo(parallelFirstCurves.First());
                    XYZ dir = parallelFirstCurves.First().GetNormalInXoy(firstCurve.GetEndPoint(0));
                    var transform = Transform.CreateTranslation(dir * distance);

                    for (int i = 0; i < parallelFirstCurves.Count; i++)
                    {
                        parallelFirstCurves[i] = parallelFirstCurves[i].CreateTransformed(transform);
                    }

                    //自后向前一次重新加入
                    var lastParallelCurve = parallelFirstCurves.Last();
                    ExtendTwoCurves(ref notParallelFirstCurve, ref lastParallelCurve);
                    lines.Insert(1, notParallelFirstCurve);

                    for (int i = parallelFirstCurves.Count - 1; i > -1; i--)
                    {
                        lines.Insert(1, parallelFirstCurves[i]);
                    }
                }
            }
            #endregion

            return CurveLoop.Create(lines);
        }

        private static bool DealWithCurrTwoCurvres(ref Curve currCurve, ref Curve preCurve, out Line tempLine, double tolerance)
        {
            bool isMovedCurrCurve = false;
            tempLine = null;

            if (preCurve.GetEndPoint(1).IsAlmostEqualTo(currCurve.GetEndPoint(0)))
            {//首尾相连，不需要处理
            }
            else
            {//需要处理
                bool isParallel = CurveUtils.IsParallelTo(preCurve, currCurve);
                if (isParallel)
                {//平行，求距离，距离小于300mm，将第二条直线移动到第一条直线的延长线上；否则直接相连
                    double distance = preCurve.DistanceTo(currCurve);
                    if (distance <= tolerance /*&& distance > 1e-6*/)
                    {
                        isMovedCurrCurve = true;
                        MoveCurrCurveAndExtend(ref currCurve, preCurve, distance);
                    }
                    else
                    {
                        //直接相连
                        var pts = CurveUtils.GetNearestPointsByTwoLine(preCurve, currCurve);
                        if (!pts[0].IsAlmostEqualTo(pts[1]))
                        {
                            tempLine = Line.CreateBound(pts[0], pts[1]);
                        }
                    }

                }
                else
                {//不平行，延长两条线，求交点
                    ExtendTwoCurves(ref currCurve, ref preCurve);
                }
            }

            return isMovedCurrCurve;
        }

        private static void MoveCurrCurveAndExtend(ref Curve currCurve, Curve preCurve, double distance)
        {
            //先把第二条直线移动到和前一条直线共线
            XYZ dir = currCurve.GetNormalInXoy(preCurve.GetEndPoint(0));
            currCurve = currCurve.CreateTransformed(Transform.CreateTranslation(dir * distance));

            var pts = CurveUtils.GetNearestPointsByTwoLine(preCurve, currCurve);
            if (!pts[0].IsAlmostEqualTo(pts[1]))
            {
                var starPoint = pts[0];
                var endPoint = currCurve.GetEndPoint(1);
                Curve currTemp = currCurve.Clone();
                currTemp.MakeUnbound();

                IntersectionResult intersectionResult;
                intersectionResult = currTemp.Project(starPoint);
                double start = intersectionResult.Parameter;
                intersectionResult = currTemp.Project(endPoint);
                double end = intersectionResult.Parameter;
                currTemp.MakeBound(start, end);
                currCurve = currTemp.Clone();
            }
        }

        private static void ExtendTwoCurves(ref Curve currCurve, ref Curve preCurve)
        {
            Curve preTemp = preCurve.Clone();
            preTemp.MakeUnbound();
            Curve currTemp = currCurve.Clone();
            currTemp.MakeUnbound();

            _ = preTemp.Intersect(currTemp, out IntersectionResultArray resultArray);
            XYZ point = resultArray.get_Item(0).XYZPoint;  //交点


            IntersectionResult intersectionResult_0, intersectionResult_1;
            double start, end;

            //preCurve
            intersectionResult_0 = preTemp.Project(preCurve.GetEndPoint(0));
            start = intersectionResult_0.Parameter;
            intersectionResult_1 = preTemp.Project(point);
            end = intersectionResult_1.Parameter;
            preTemp.MakeBound(start, end);
            preCurve = preTemp.Clone();

            //currCurve
            intersectionResult_0 = currTemp.Project(point);
            start = intersectionResult_0.Parameter;
            intersectionResult_1 = currTemp.Project(currCurve.GetEndPoint(1));
            end = intersectionResult_1.Parameter;
            currTemp.MakeBound(start, end);
            currCurve = currTemp.Clone();
        }

        private static XYZ[] GetNearestPointsByTwoLine(Curve line1, Curve line2)
        {
            var p1 = line1.GetEndPoint(0);
            var p2 = line1.GetEndPoint(1);
            XYZ[] c1 = new XYZ[2] { p1, p2 };

            var p3 = line2.GetEndPoint(0);
            var p4 = line2.GetEndPoint(1);
            XYZ[] c2 = new XYZ[2] { p3, p4 };

            XYZ pt1 = null, pt2 = null;
            double minDis = double.MaxValue;
            foreach (var i in c1)
            {
                foreach (var j in c2)
                {
                    var distance = i.DistanceTo(j);
                    if (distance < minDis)
                    {
                        minDis = distance;
                        pt1 = i;
                        pt2 = j;
                    }
                }
            }

            return new XYZ[2] { pt1, pt2 };
        }
        #endregion

        #endregion

        #region Point Related
        /// <summary>
        /// 获取一个点在XOY平面上的投影点
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ ProjectXoy(this XYZ xyz)
        {
            if (xyz == null)
            {
                return null;
            }
            return new XYZ(xyz.X, xyz.Y, 0);
        }

        /// <summary>
        /// ToUVPoint
        /// </summary>
        /// <param name="xYZ"></param>
        /// <returns></returns>
        public static UV ToUV(this XYZ xYZ)
        {
            if (xYZ == null)
                return null;

            UV uv = new UV(xYZ.X, xYZ.Y);
            return uv;
        }

        #endregion
    }
}
