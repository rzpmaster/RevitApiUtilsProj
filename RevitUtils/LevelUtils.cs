using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    public static class LevelUtils
    {
        /// <summary>
        /// 获得当前文件中的所有标高，并排序
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Level> GetAllLevels(this Document doc)
        {
            FilteredElementCollector levelFilter = new FilteredElementCollector(doc);
            ICollection<Element> levelCollect = levelFilter.OfClass(typeof(Level)).ToElements();

            List<Level> levelList = new List<Level>();
            foreach (Element item in levelCollect)
            {
                Level level = item as Level;
                levelList.Add(level);
            }

            levelList.Sort(delegate (Level a, Level b) { return a.Elevation.CompareTo(b.Elevation); });

            return levelList;
        }

        /// <summary>
        /// 获取元素的标高
        /// 普通方法获取不到的时候，就获取第一个比这个元素中心点低的标高
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Level ElementLevel(this Element element)
        {
            var doc = element.Document;
            ElementId levelId = element.LevelId;
            if (doc.GetElement(levelId) == null)
            {
                var levelPara = element.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM);
                if (levelPara == null)
                {
                    levelPara = element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
                }
                if (levelPara != null)
                {
                    levelId = levelPara.AsElementId();
                }

                if (levelId.IntegerValue == -1)
                {
                    // General get level method
                    var bbox = element.get_BoundingBox(null);
                    double zValue = (bbox.Min.Z + bbox.Max.Z) / 2;

                    var levels = GetAllLevels(doc);
                    if (zValue <= levels.FirstOrDefault()?.Elevation)
                    {
                        return levels.FirstOrDefault();
                    }
                    if (zValue >= levels.LastOrDefault()?.Elevation)
                    {
                        return levels.LastOrDefault();
                    }

                    for (int i = 0; i < levels.Count; i++)
                    {
                        if (levels[i].Elevation >= zValue)
                        {
                            return levels[i - 1];
                        }
                    }
                }
            }
            return doc.GetElement(levelId) as Level;
        }
    }
}
