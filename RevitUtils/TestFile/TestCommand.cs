using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace RevitUtils.TestFile
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                // TODO: 测试代码这里实现

            }
            catch
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SelectedTestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                // TODO: 测试代码这里实现
                ViewUtils.HighLightElements(uidoc, new List<ElementId>() { new ElementId(2170659) });

            }
            catch
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
