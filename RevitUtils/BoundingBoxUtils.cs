using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    public static class BoundingBoxUtils
    {
        /// <summary>
        /// 获取能包含所有给定元素的BoundingBox
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static BoundingBoxXYZ GetElementsMaxBounding(List<Element> elements)
        {
            var bBoxs = elements.Select(e => e.get_BoundingBox(null)).SkipWhile(e => e == null).ToList();

            return GetElementsMaxBounding(bBoxs);
        }

        /// <summary>
        /// 获得能包围所有包围盒的BoundingBox
        /// </summary>
        /// <param name="bBoxs"></param>
        /// <returns></returns>
        public static BoundingBoxXYZ GetElementsMaxBounding(List<BoundingBoxXYZ> bBoxs)
        {
            BoundingBoxXYZ retBBox = new BoundingBoxXYZ() { Enabled = false };

            bBoxs.ForEach(bbox => retBBox.ExpandToContain(bbox));
            return retBBox;
        }

        /// <summary>
        /// 获得element的BoundingBoxXYZ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static BoundingBoxXYZ GetBoundingBox(this Element element)
        {
            if (null == element)
                return null;

            // 获取实例的BoundingBox
            if (element is FamilyInstance)
            {
                var bBox = new BoundingBoxXYZ() { Enabled = false };
                var solids = GeometryUtils.GetSolids(element);
                foreach (var solid in solids)
                {
                    if (solid.Faces.Size <= 0 || solid.Volume <= 1e-6)
                    {
                        continue;
                    }

                    foreach (Edge e in solid.Edges)
                    {
                        var crv = e.AsCurve();
                        bBox.ExpandToContain(crv.GetEndPoint(0));
                        bBox.ExpandToContain(crv.GetEndPoint(1));
                    }
                }

                return bBox;
            }

            // 获取一般的BoundingBox
            Options ops = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geoElem = element.get_Geometry(ops);

            return geoElem?.GetBoundingBox();
        }

        /// <summary>
        /// whether the point is in bbox
        /// </summary>
        /// <param name="bb"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool Contain(this BoundingBoxXYZ bb, XYZ p)
        {
            if (bb.Max.X > p.X && bb.Max.Y > p.Y && bb.Max.Z > p.Z &&
                bb.Min.X < p.X && bb.Min.Y < p.Y && bb.Min.Z < p.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Expand the given bounding box to include and contain the given point.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, XYZ p)
        {
            bb.Min = new XYZ(Math.Min(bb.Min.X, p.X),
              Math.Min(bb.Min.Y, p.Y),
              Math.Min(bb.Min.Z, p.Z));

            bb.Max = new XYZ(Math.Max(bb.Max.X, p.X),
              Math.Max(bb.Max.Y, p.Y),
              Math.Max(bb.Max.Z, p.Z));
        }

        /// <summary>
        /// Expand the given bounding box to include and contain the given other one.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, BoundingBoxXYZ other)
        {
            bb.ExpandToContain(other.Min);
            bb.ExpandToContain(other.Max);
        }
    }
}
