using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RevitUtils
{
    /// <summary>
    /// MepCurve Class is Inherited from HostObject
    /// and its subclass has
    /// Duct(风管) FlexDuct(软风管) Pipe(水管) FlexPipe(软水管) Wire(导线) 
    /// CableTrayConduitBase(桥架 导管)
    /// InsulationLiningBase(管道外包内衬)
    /// MEPAnalyticalConnection
    /// </summary>
    public static class MepCurveUtils
    {
        /// <summary>
        /// 获得机电管线的连接件
        /// </summary>
        /// <param name="mepCurve"></param>
        /// <returns></returns>
        public static ConnectorSet GetConnectors(MEPCurve mepCurve)
        {
            return mepCurve.ConnectorManager.Connectors;
        }

        //TODO:break打断
    }

    public static class ConnectorUtils
    {
        /// <summary>
        /// 获得一个元素的所有连接件
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ConnectorSet GetConnectors(Element element)
        {
            if (element == null) return null;
            FamilyInstance fi = element as FamilyInstance;
            if (fi != null && fi.MEPModel != null)
            {
                return fi.MEPModel.ConnectorManager.Connectors;
            }
            MEPSystem system = element as MEPSystem;
            if (system != null)
            {
                return system.ConnectorManager.Connectors;
            }
            MEPCurve mep = element as MEPCurve;
            if (mep != null)
            {
                return mep.ConnectorManager.Connectors;
            }
            return null;
        }

        /// <summary>
        /// 返回元素的扫有连接件中的任意一个
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <remarks>经常用于只有一个连接件的元素</remarks>
        public static Connector GetConnector(Element element)
        {
            var conns = GetConnectors(element);
            if (conns != null)
            {
                return conns.Cast<Connector>().FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 通过连接件指向的方向，获得连接件
        /// </summary>
        /// <param name="element"></param>
        /// <param name="connectorOrigin"></param>
        /// <returns></returns>
        public static Connector GetConnectorByDirection(Element element, XYZ connectorOrigin)
        {
            ConnectorSet connectors = GetConnectors(element);
            foreach (Connector conn in connectors)
            {
                if (conn.ConnectorType == ConnectorType.Logical)
                    continue;
                if (conn.GetConnectorDirection().IsAlmostEqualTo(connectorOrigin))
                    return conn;
            }
            return null;
        }

        /// <summary>
        /// 通过连接件的LocationPoint，获得连接件
        /// </summary>
        /// <param name="element"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Connector GetConnectorByPoint(Element element, XYZ point)
        {
            ConnectorSet connectors = GetConnectors(element);
            foreach (Connector conn in connectors)
            {
                if (conn.ConnectorType == ConnectorType.Logical)
                    continue;
                if (conn.Origin.IsAlmostEqualTo(point))
                    return conn;
            }
            return null;
        }

        /// <summary>
        /// 获得连接件指向的方向
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static XYZ GetConnectorDirection(this Connector conn)
        {
            Transform t = conn.CoordinateSystem;
            return new XYZ(t.BasisZ.X, t.BasisZ.Y, t.BasisZ.Z);
        }

        /// <summary>
        /// 获得某个元素的某个连接件上连接的其他元素
        /// </summary>
        /// <param name="element"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<Element> GetConnLinkedElements(Element element, Connector conn)
        {
            List<Element> elements = new List<Element>();
            ConnectorSetIterator connectorSetIterator = conn.AllRefs.ForwardIterator();
            while (connectorSetIterator.MoveNext())
            {
                Connector connref = connectorSetIterator.Current as Connector;
                Element elem = connref.Owner;
                if (elem != null && elem.UniqueId != element.UniqueId)
                {
                    elements.Add(elem);
                }
            }
            return elements;
        }
    }
}
