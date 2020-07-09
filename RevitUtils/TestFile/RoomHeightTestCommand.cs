using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitUtils.InterfaceReated;

namespace RevitUtils.TestFile
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RoomHeightTestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                // TODO: 测试代码这里实现
                var roomref = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.LinkedElement, new LinkRoomSelectionFilter(), "请选择一个链接文件中的房间");
                Room room = LinkedElementUtils.GetLinkedDocumnet(uidoc.Document, roomref).GetElement(roomref.LinkedElementId) as Room;

                var rst = RoomUtils.TryGetRoomHeight(room, out double roomHeight, uidoc.Document);

                string msg = string.Empty;
                switch (rst)
                {
                    case -1:
                        msg = "参数法";
                        break;
                    case 0:
                        msg = "标高法";
                        break;
                    case 1:
                        msg = "射线法";
                        break;
                    default:
                        break;
                }
                msg += $"获取的房间高度为{roomHeight.FeetToMM()}";

                TaskDialog.Show("房间高度测试", msg);

                return Result.Succeeded;
            }
            catch
            {
                return Result.Cancelled;
            }
        }
    }
}
