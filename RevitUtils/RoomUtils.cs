using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    public static class RoomUtils
    {
        /// <summary>
        /// 获得房间地板到楼板的距离，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <param name="roomHeight">房间高度</param>
        /// <returns>
        /// -1 表示结果不可靠，  返回的是 参数法 获取的高度 ：当前房间的参数“房间标识高度”的值
        /// 0  表示结果一般可靠，返回的是 标高法 获取的高度 ：房间上下两层的标高之间的距离
        /// 1  表示结果很可靠，  返回的是 射线法 获取的高度 ：房间上下两个楼板之间的距离
        /// </returns>
        public static int TryGetRoomHeight(this Room room, out double roomHeight)
        {
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

            //获取房间的楼板
            var bbox = GetRoomMaxBoundingBox(room, elevationHeight);
            var floors = GetElementsInRoomByBbox(BuiltInCategory.OST_Floors, bbox, room.Document);
            if (floors.Count == 0)
            {//没有楼板，射线法失败
            }
            else
            {
                //以下使用射线法，得到准确的房间高度
                var point = GetRoomCenterPoint(room);
                point += new XYZ(0, 0, elevationHeight / 2);
                roomHeight = GetRoomHeightByRay(room, point);
                if (!double.IsNaN(roomHeight)) return 1;
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
        /// <param name="pointInRoom">房间内一点，请确保该点在房间上下两个楼板的标高内，否则计算值错误</param>
        /// <returns></returns>
        public static double GetRoomHeightByRay(this Room room, XYZ pointInRoom)
        {
            var topHeight = GetRoomFloorDistanceByRay(room, pointInRoom, XYZ.BasisZ);
            if (double.IsNaN(topHeight)) return double.NaN;
            var bottomHeight = GetRoomFloorDistanceByRay(room, pointInRoom, XYZ.BasisZ.Negate());
            if (double.IsNaN(bottomHeight)) return double.NaN;

            return topHeight + bottomHeight;
        }

        /// <summary>
        /// 射线法获取距离楼板的距离，返回给定点到第一个楼板的距离
        /// </summary>
        /// <param name="room"></param>
        /// <param name="pointInRoom"></param>
        /// <param name="rayDirection"></param>
        /// <returns></returns>
        private static double GetRoomFloorDistanceByRay(Room room, XYZ pointInRoom, XYZ rayDirection)
        {
            FilteredElementCollector collector = new FilteredElementCollector(room.Document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            ElementFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(floorFilter, FindReferenceTarget.Element, view3D);
            referenceIntersector.FindReferencesInRevitLinks = room.Document.IsLinked;
            var referenceWithContext = referenceIntersector.FindNearest(pointInRoom, rayDirection);
            if (referenceWithContext != null)
            {
                if ((referenceWithContext.Proximity > MathHelper.Eps) &&
                    (referenceWithContext.Proximity < 50 * 1000 * MathHelper.Mm2Feet)) //大于50m就算获取失败
                {
                    return referenceWithContext.Proximity;
                }
            }
            //throw new InvalidOperationException("房间上方没有楼板，无法使用射线法");
            return double.NaN;
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
        /// 通过房间的bbox 使用BoundingBoxIntersectsFilter 获得房间内的构建
        /// 速度快，但是不够准确
        /// </summary>
        /// <param name="room"></param>
        /// <param name="builtInCategory">目标构建的类型</param>
        /// <param name="document">目标构建该document中找，默认为room.Document，如果给定房间和目标构建不在同一个文件中（例如有一个在链接文件中），需要给定这个参数</param>
        /// <returns></returns>
        public static IList<Element> GetElementsInRoomByBbox(BuiltInCategory builtInCategory, Outline outline, Document document)
        {
            if (outline == null) return new List<Element>();

            FilteredElementCollector collector = new FilteredElementCollector(document);
            var bboxFilter = new BoundingBoxIntersectsFilter(outline);
            var list = collector.OfCategory(builtInCategory)
                                .WhereElementIsNotElementType()
                                .WherePasses(bboxFilter).ToElements();
            return list;
        }

        /// <summary>
        /// 获取房间标高平面上，房间中心点
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static XYZ GetRoomCenterPoint(Room room)
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
        public static Outline GetRoomMaxBoundingBox(Room room, double height)
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
        public static IList<CurveLoop> GetRoomBoundaryAsCurveLoopArray(Room room)
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

                return loops;
            }

            //没有找到底面
            return null;
        }

        /// <summary>
        /// 获取房间Solid
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Solid GetRoomSolid(Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(room.Document, bndOpt);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room); // compute the room geometry
            Solid roomSolid = results.GetGeometry(); // get the solid representing the room's geometry

            return roomSolid;
        }
    }
}
