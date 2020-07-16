using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.IFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    /// <summary>
    /// SpatialElement's subclass has
    /// Architecture.Room
    /// Area
    /// Mechanical.Space
    /// </summary>
    public static class RoomUtils
    {
        /// <summary>
        /// 获得房间地板到楼板的距离，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDocument">当前文件，如果是房间是链接文件中的房间，该参数必须是当前文件</param>
        /// <param name="roomHeight">房间高度</param>
        /// <returns>
        /// -1 表示结果不可靠，  返回的是 参数法 获取的高度 ：当前房间的参数“房间标识高度”的值
        /// 0  表示结果一般可靠，返回的是 标高法 获取的高度 ：房间上下两层的标高之间的距离
        /// 1  表示结果很可靠，  返回的是 射线法 获取的高度 ：房间上下两个楼板之间的距离
        /// </returns>
        /// <remarks>
        /// 当房间是连接文件中的房间，但是没有传递当前文件currDocument参数时，跳过射线法
        /// </remarks>
        public static int TryGetRoomHeight(this Room room, out double roomHeight, Document currDocument = null)
        {
            bool canUseRayMethod = true;
            if (room.Document.IsLinked && currDocument == null)
                canUseRayMethod = false;

            var paramHeight = GetRoomHeightByParam(room);   //从房间参数中获取的房间高度

            var elevationHeight = GetRoomHeightByAdjacentElevation(room);
            bool isTopLevel = double.IsNaN(elevationHeight);
            if (isTopLevel)
            {//避免是顶层导致的错误
                elevationHeight = room.Level.Elevation + 5000 * MathHelper.Mm2Feet;
            }
            else if (paramHeight > elevationHeight)
            {//避免相邻的标高太接近而报错
                elevationHeight = GetRoomHeightByAdjacentElevation(room, paramHeight);
                if (double.IsNaN(elevationHeight)) elevationHeight = room.Level.Elevation + 5000 * MathHelper.Mm2Feet;
            }

            if (canUseRayMethod)
            {
                //获取房间的楼板
                var bbox = GetRoomMaxBoundingBox(room, elevationHeight);
                var floors = room.Document.GetElementsByBbox(bbox, BuiltInCategory.OST_Floors);
                if (floors.Count == 0)
                {//没有楼板，射线法失败
                }
                else
                {
                    //以下使用射线法，得到准确的房间高度
                    var point = GetRoomCenterPoint(room);
                    point += new XYZ(0, 0, elevationHeight / 2);

                    roomHeight = GetRoomHeightByRay(room, point, currDocument);
                    if (!double.IsNaN(roomHeight)) return 1;
                }
            }

            //射线法失败
            if (isTopLevel)
            {//如果是顶层，说明标高法失败，使用参数法
                roomHeight = paramHeight;
                return -1;
            }
            else
            {//标高法
                roomHeight = elevationHeight;
                return 0;
            }
        }

        /// <summary>
        /// 射线法获取房间高度，单位foot 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="pointInRoom">房间内一点，请确保该点在房间上下两个楼板范围内，否则计算值错误</param>
        /// <param name="currDocument">当前文件，射线法需要用到</param>
        /// <returns></returns>
        public static double GetRoomHeightByRay(this Room room, XYZ pointInRoom, Document currDocument)
        {
            if (room.Document.IsLinked && currDocument == null)
                throw new InvalidOperationException("currDocument为空，如果房间是链接文件中的房间，必须传递当前文件currDocument参数");

            ElementFilter elementFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            //射线法需要的三维视图，很关键，找不好就会找不到想要的结果
            FilteredElementCollector collector = new FilteredElementCollector(currDocument);
            //待改进
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(v3 => !(v3.IsTemplate));

            var referenceIntersector = RevitExtensions.GetReferenceIntersector(
                view3D,
                room.Document.IsLinked,
                findReferenceTarget: FindReferenceTarget.All,
                targetElementIds: null,
                elementFilter: elementFilter);

            var topReference = referenceIntersector.FindNearest(pointInRoom, XYZ.BasisZ);
            if (topReference == null) return double.NaN;

            var bottomReference = referenceIntersector.FindNearest(pointInRoom, XYZ.BasisZ.Negate());
            if (bottomReference == null) return double.NaN;

            return topReference.Proximity + bottomReference.Proximity;
        }

        /// <summary>
        /// 标高法获取房间高度，计算给定房间上下 两个标高的距离，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <remarks>如果存在夹层标高，或者相邻的标高距离的很近，有可能会返回不正确的标高</remarks>
        public static double GetRoomHeightByAdjacentElevation(this Room room, double tolerance = 100 * MathHelper.Mm2Feet)
        {
            List<Level> allLevel = LevelUtils.GetAllLevels(room.Document);
            var upperLevel = allLevel.FirstOrDefault(x => x.Elevation - room.Level.Elevation >= tolerance);
            if (upperLevel == null)
            {
                //throw new InvalidOperationException("该房间没有上层标高，无法通过标高获取房间高度");
                //return room.Level.Elevation + 5000 * MathHelper.Mm2Feet;    //避免顶层报错
                return double.NaN;
            }
            return upperLevel.Elevation - room.Level.Elevation;
        }

        /// <summary>
        /// 参数法获取房间高度，通过获取房间的参数“房间标识高度”值，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <remarks>由于</remarks>
        public static double GetRoomHeightByParam(this Room room)
        {
            //返回当前房间的参数“房间标识高度”的值
            return room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
        }

        /// <summary>
        /// 获取房间标高平面上，房间中心点
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static XYZ GetRoomCenterPoint(this Room room)
        {
            BoundingBoxXYZ box = room.get_BoundingBox(null);
            XYZ center = box.Min.Add(box.Max).Multiply(0.5);
            return new XYZ(center.X, center.Y, room.Level.Elevation);
        }

        /// <summary>
        /// 获得房间的最大 BoundingBox
        /// </summary>
        /// <param name="room"></param>
        /// <param name="height">房间高度（需要拉伸高度的：从房间标高的Elevation拉升到指定高度）</param>
        /// <returns></returns>
        public static Outline GetRoomMaxBoundingBox(this Room room, double height)
        {
            //var height = GetRoomHeightByAdjacentElevation(room);
            var level = room.Level.Elevation;
            height += level;//房间的总高度

            var bbox = room.get_BoundingBox(room.Document.ActiveView);
            var new_posation = new XYZ(bbox.Max.X, bbox.Max.Y, height);//将bbox的高度拉高至房间最高的墙的高度
            return new Outline(bbox.Min, new_posation);
        }

        /// <summary>
        /// 通过IFC工具 获取Room的Curveloop属性
        /// 房间的CsurveLoop 可能有自交的情况（自交的情况下不能拉伸Solid），通过调整SpatialElementBoundaryLocation.Center可以消除
        /// </summary>
        /// <param name="room"></param>
        /// <returns>返回该房间边界的CurveLoop，包括墙、分隔线、柱等</returns>
        public static IList<CurveLoop> GetRoomBoundaryAsCurveLoopArray(this Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            foreach (SpatialElementBoundaryLocation spl in Enum.GetValues(typeof(SpatialElementBoundaryLocation)))
            {
                //获取房间边界的定位点，可以是边界、中心、核心层中心、核心层边界等
                bndOpt.SpatialElementBoundaryLocation = spl;
                try
                {
                    var loops = ExporterIFCUtils.GetRoomBoundaryAsCurveLoopArray(room, bndOpt, true);

                    // 验证CurveLoop是否合法（因为有可能存在自交的情况）
                    GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 10);

                    return loops;
                }
                catch (Exception)
                {
                }
            }

            return GetRoomBoundaryBySolid(room);
        }

        /// <summary>
        /// 通过房间 Solid 获取房间底部轮廓
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        [Obsolete("备用方法,请勿直接使用!", false)]
        public static IList<CurveLoop> GetRoomBoundaryBySolid(Room room)
        {
            var solid = GetRoomSolid(room);
            var bottomFace = FaceUtils.GetBottomFace(solid);
            if (bottomFace != null)
            {
                var loops = bottomFace.GetEdgesAsCurveLoops();

                for (int i = 0; i < loops.Count; i++)
                {
                    if (loops[i].IsOpen())
                        loops[i] = loops[i].ValidateCurveLoop();
                }

                return loops;
            }

            //没有找到底面
            return null;
        }

        /// <summary>
        /// 使用空间计算工具计算房间Solid
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <remarks>轮廓上没问题,在高度上可能会出错</remarks>
        public static Solid GetRoomSolid(this Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(room.Document, bndOpt);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room); // compute the room geometry
            Solid roomSolid = results.GetGeometry(); // get the solid representing the room's geometry

            return roomSolid;
        }

        /// <summary>
        /// 获得房间的拉伸实体Solid
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDoc">当前文件</param>
        /// <returns></returns>
        /// <remarks>当房间是连接文件中的房间，但是没有传递当前文件currDocument参数时，跳过射线法计算房间高度</remarks>
        public static Solid GetRoomActualSolid(this Room room, Document currDoc = null)
        {
            var loops = GetRoomBoundaryAsCurveLoopArray(room);
            if (loops == null)
            {
                return null;
            }

            TryGetRoomHeight(room, out double roomHeight, currDoc);
            return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, roomHeight);
        }

        /// <summary>
        /// 获得房间边界及边界组成元素
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDocument">当前文件，射线法需要用到</param>
        /// <returns>内层List为CurveLoop组成Curve,外层IList是组成CurveLoop的个数</returns>
        public static IList<List<RoomBoundary>> GetRoomSurroundingElements(this Room room, Document currDocument)
        {
            if (room.Document.IsLinked && currDocument == null)
                throw new InvalidOperationException("currDocument为空，如果房间是链接文件中的房间，必须传递当前文件currDocument参数");

            IList<IList<BoundarySegment>> segmentsloops = null;
            IList<CurveLoop> curveLoop = null;
            var bndOpt = new SpatialElementBoundaryOptions();
            foreach (SpatialElementBoundaryLocation spl in Enum.GetValues(typeof(SpatialElementBoundaryLocation)))
            {
                //获取房间边界的定位点，可以是边界、中心、核心层中心、核心层边界等
                bndOpt.SpatialElementBoundaryLocation = spl;
                try
                {
                    segmentsloops = room.GetBoundarySegments(bndOpt);
                    if (segmentsloops != null)
                    {
                        curveLoop = segmentsloops.Select(e =>
                        {
                            var curves = e.Select(l => l.GetCurve()).ToList();
                            return CurveLoop.Create(curves);
                        }).ToList();

                        // 验证CurveLoop是否合法（因为有可能存在自交的情况）
                        GeometryCreationUtilities.CreateExtrusionGeometry(curveLoop, XYZ.BasisZ, 10);
                        break;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (segmentsloops == null)
            {
                return null;
            }

            IList<List<RoomBoundary>> rstBoundaryList = new List<List<RoomBoundary>>();
            for (int i = 0; i < segmentsloops.Count; i++)
            {
                bool isCounterclockwise = curveLoop[i].IsCounterclockwise(XYZ.BasisZ);
                var elements = GetElementsByBoundarySegments(currDocument, segmentsloops[i], isCounterclockwise);
                rstBoundaryList.Add(elements);
            }

            return rstBoundaryList;
        }

        private static List<RoomBoundary> GetElementsByBoundarySegments(Document document, IList<BoundarySegment> loop, bool isCounterclockwise)
        {
            List<RoomBoundary> elements = new List<RoomBoundary>();
            foreach (BoundarySegment segment in loop)
            {
                if (segment != null)
                {
                    Element element = document.GetElement(segment.ElementId);
                    if (element == null)
                    {
                        element = document.GetElement(segment.LinkElementId);
                    }

                    Curve segmentCure = segment.GetCurve();
                    if (element == null)
                    {//使用射线法找到该 segment 对应的元素
                        element = GetSegmentElementByRay(document, segmentCure, isCounterclockwise);
                    }
                    elements.Add(new RoomBoundary() { BoundaryCurve = segmentCure, BoundaryElement = element, IsOuterBoundary = isCounterclockwise });
                }
            }

            return elements;
        }

        private static Element GetSegmentElementByRay(Document document, Curve segmentCure, bool isCounterclockwise)
        {
            double stepInRoom = 0.1;
            XYZ direction = (segmentCure.GetEndPoint(1) - segmentCure.GetEndPoint(0)).Normalize();
            var leftDirection = new XYZ(-direction.Y, direction.X, direction.Z);
            XYZ upDir = 1 * XYZ.BasisZ;

            XYZ toRoomVec = isCounterclockwise ? stepInRoom * leftDirection : stepInRoom * leftDirection.Negate();
            XYZ pointBottomInRoom = segmentCure.Evaluate(0.5, true) + toRoomVec;

            XYZ startPoint = pointBottomInRoom + upDir;
            XYZ toWallDir = isCounterclockwise ? leftDirection.Negate() : leftDirection;

            //默认过滤 墙 柱子 门 幕墙嵌板
            var filters = new List<ElementFilter>{
                new ElementCategoryFilter( BuiltInCategory.OST_Walls),
                new ElementCategoryFilter(BuiltInCategory.OST_Columns),
                new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels),   //幕墙嵌板
            };
            var orFilter = new LogicalOrFilter(filters);

            //射线法需要的三维视图，很关键，找不好就会找不到想要的结果
            FilteredElementCollector collector = new FilteredElementCollector(document);
            //待改进
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(v3 => !(v3.IsTemplate));

            var referenceIntersector = RevitExtensions.GetReferenceIntersector(
                view3D,
                true,
                findReferenceTarget: FindReferenceTarget.All,
                targetElementIds: null,
                elementFilter: orFilter);

            var reference = referenceIntersector.FindNearest(startPoint, toWallDir);

            Element boundaryElement = null;
            if (reference != null && reference.Proximity < MathHelper.Eps + stepInRoom)
            {
                boundaryElement = reference.GetElementByReferenceWithContext(document);
            }
            return boundaryElement;
        }

        /// <summary>
        /// 获取房间最外圈的CurveLoop
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static CurveLoop GetRoomMaxCurveLoop(this Room room)
        {
            var loops = GetRoomBoundaryAsCurveLoopArray(room);
            if (loops != null && loops.Count > 0)
            {
                return loops.OrderBy(x =>
                ExporterIFCUtils.ComputeAreaOfCurveLoops(new List<CurveLoop> { x })).LastOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 获取房间最外圈的元素
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDocument">当前文件，射线法需要用到</param>
        /// <returns></returns>
        public static List<Element> GetRoomMaxSurroundingElements(this Room room, Document currDocument)
        {
            var allElements = GetRoomSurroundingElements(room, currDocument);
            return allElements.FirstOrDefault(rb => rb.FirstOrDefault()?.IsOuterBoundary ?? false)?.Select(e => e.BoundaryElement).ToList();
        }

        /// <summary>
        /// 判断给定房间是否有吊顶
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDocument">当前文件</param>
        /// <returns></returns>
        /// <remarks>当房间是连接文件中的房间，但是没有传递当前文件currDocument参数时，跳过射线法计算房间高度一</remarks>
        public static bool HasCeiling(this Room room, Document currDocument = null)
        {
            return HasCeiling(room, out _, currDocument);
        }

        /// <summary>
        /// 判断房间是否有吊顶
        /// </summary>
        /// <param name="room"></param>
        /// <param name="ceilings">房间内的吊顶</param>
        /// <param name="currDocument">当前文件</param>
        /// <returns></returns>
        /// <remarks>当房间是连接文件中的房间，但是没有传递当前文件currDocument参数时，跳过射线法计算房间高度一</remarks>
        public static bool HasCeiling(this Room room, out IList<Element> ceilings, Document currDocument = null)
        {
            TryGetRoomHeight(room, out double height, currDocument);

            var bbox = room.GetRoomMaxBoundingBox(height);
            ceilings = room.Document.GetElementsByBbox(bbox, BuiltInCategory.OST_Ceilings);
            return ceilings.Count > 0;
        }

        /// <summary>
        /// 计算房间吊顶到房间地面的高度，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <param name="currDocument">当前文件</param>
        /// <returns></returns>
        /// <remarks>当房间是连接文件中的房间，但是没有传递当前文件currDocument参数时，跳过射线法计算房间高度一</remarks>
        public static double? GetRoomCeilingHeight(this Room room, Document currDocument = null)
        {
            bool canUseRayMethod = true;
            if (room.Document.IsLinked && currDocument == null)
                canUseRayMethod = false;

            var hasCeiling = HasCeiling(room, out IList<Element> ceilings, currDocument);
            if (!hasCeiling) return null;

            var levels = ceilings.Select(e => e.ElementLevel().Elevation).ToList();
            levels.Sort();
            var height = levels.FirstOrDefault(e => e > room.Level.Elevation) - room.Level.Elevation;

            if (canUseRayMethod)
            {
                // 再次使用射线法查找准确的高度,防止找到的天花板在相邻的房间内而导致的错误
                var point = GetRoomCenterPoint(room);
                point += new XYZ(0, 0, height / 2);
                var ceilingHeight = GetRoomCeilingHeightByRay(room, point, currDocument);

                if (!double.IsNaN(ceilingHeight)) return ceilingHeight;
            }
            return height;
        }

        /// <summary>
        /// 射线法获取房间吊顶高度，单位foot 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="pointInRoom">房间内一点，请确保该点在房间下楼板和上天花板内，否则计算值错误</param>
        /// <param name="currDocument">当前文件，射线法需要用到</param>
        /// <returns></returns>
        public static double GetRoomCeilingHeightByRay(this Room room, XYZ pointInRoom, Document currDocument)
        {
            if (room.Document.IsLinked && currDocument == null)
                throw new InvalidOperationException("如果房间是链接文件中的房间，必须传递当前文件currDocument参数");

            ElementFilter elementFilter;
            ReferenceIntersector referenceIntersector;

            //向上找天花板
            elementFilter = new ElementCategoryFilter(BuiltInCategory.OST_Ceilings);

            //射线法需要的三维视图，很关键，找不好就会找不到想要的结果
            FilteredElementCollector collector = new FilteredElementCollector(currDocument);
            //待改进
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(v3 => !(v3.IsTemplate));

            referenceIntersector = RevitExtensions.GetReferenceIntersector(
                view3D,
                room.Document.IsLinked,
                findReferenceTarget: FindReferenceTarget.All,
                targetElementIds: null,
                elementFilter: elementFilter);

            var topReference = referenceIntersector.FindNearest(pointInRoom, XYZ.BasisZ);
            if (topReference == null) return double.NaN;

            //向下找楼板
            elementFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            referenceIntersector.SetFilter(elementFilter);

            var bottomReference = referenceIntersector.FindNearest(pointInRoom, XYZ.BasisZ.Negate());
            if (bottomReference == null) return double.NaN;

            return topReference.Proximity + bottomReference.Proximity;
        }

        /// <summary>
        /// 判断点是否在房间内
        /// </summary>
        /// <param name="room"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsPointInRoom(this Room room, XYZ point)
        {
            var loops = GetRoomBoundaryAsCurveLoopArray(room);
            return loops.IsPointInPolygon(point);
        }
    }

    public sealed class RoomBoundary
    {
        public Curve BoundaryCurve { get; set; }
        public Element BoundaryElement { get; set; }
        public bool IsOuterBoundary { get; set; }
    }
}
