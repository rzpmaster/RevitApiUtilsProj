using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitUtils.DebugReated;
using System;
using System.Collections.Generic;

namespace RevitUtils.TestFile
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TestElementGenerator : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                // TODO: 测试代码这里实现
                var ids = new List<int> { 2042195 };
                ElementGenerator.CreateDirectShapes(uidoc.Document, ids);
            }
            catch(Exception ex)
            {
                TaskDialog.Show("提示",ex.ToString());
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
