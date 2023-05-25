using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITotalRoomNuumber
{
    public class MainViewViewModel : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            XYZ point = new XYZ();

            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .OfType<FamilySymbol>()
                .Where(x => x.FamilyName.Equals("Номер помещения"))
                .FirstOrDefault();

            if (familySymbol == null)
            {
                TaskDialog.Show("Ошибка", "Не найдено семейство \"Номер помещения\"");
                return Result.Cancelled;
            }

            List<Room> roomList = new List<Room>();


            roomList = GetRoomsFromCurrentSelection(doc, sel);

            if (roomList.Count == 0)
            {
                RoomSelectionFilter selFilter = new RoomSelectionFilter();
                IList<Reference> selRooms = null;
                try
                {
                    selRooms = sel.PickObjects(ObjectType.Element, selFilter, "Выберите помещения!");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }

                foreach (Reference roomRef in selRooms)
                {
                    roomList.Add(doc.GetElement(roomRef) as Room);
                }
            }

            //Вызов формы
            MainView roomNumeratorWPF = new MainView();
            roomNumeratorWPF.ShowDialog();
            if (roomNumeratorWPF.DialogResult != true)
            {
                return Result.Cancelled;
            }

            string numberPrefix = roomNumeratorWPF.NumberPrefix;
            string startFrom = roomNumeratorWPF.StartFrom;
            bool tryChech = int.TryParse(startFrom, out int cnt);
            if (!tryChech) cnt = 1;

            string selectedNumberingDirection = roomNumeratorWPF.SelectedNumberingDirection;
            switch (selectedNumberingDirection)
            {
                case "radioButton_RightAndDown":
                    //Вправо и вниз
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerXYDown())
                        .ToList();
                    break;
                case "radioButton_DownAndRight":
                    //Вниз и вправо
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerYXDown())
                        .ToList();
                    break;
                case "radioButton_RightAndUp":
                    //Вправо и вверх
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerXYUp())
                        .ToList();
                    break;
                case "radioButton_UpAndRight":
                    //Вверх и вправо
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerYXUp())
                        .ToList();
                    break;
            }

            using (Transaction ts = new Transaction(doc, "Нумерация помещений"))
            {
                ts.Start();

                foreach (Room room in roomList)
                {

                    if (numberPrefix == "" || numberPrefix == null)
                    {
                        room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set($"{cnt}");
                    }
                    else
                    {
                        room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set($"{numberPrefix}{cnt}");
                    }
                    cnt++;

                    Level level = doc.GetElement(room.LevelId) as Level;

                    point = room.get_BoundingBox(null).Max;

                    FamilyInstance num = doc.Create.NewFamilyInstance(point, familySymbol, level, StructuralType.NonStructural);
                }

                ts.Commit();
            }

            return Result.Succeeded;

        }
        private static List<Room> GetRoomsFromCurrentSelection(Document doc, Selection sel)
        {
            ICollection<ElementId> selectedIds = sel.GetElementIds();
            List<Room> tempRoomsList = new List<Room>();
            foreach (ElementId roomId in selectedIds)
            {
                if (doc.GetElement(roomId) is Room
                    && null != doc.GetElement(roomId).Category
                    && doc.GetElement(roomId).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Rooms))
                {
                    tempRoomsList.Add(doc.GetElement(roomId) as Room);
                }
            }
            return tempRoomsList;
        }

        public static XYZ GetRoomCenter(Room room)
        {
            XYZ point = null;
            point = (room.get_BoundingBox(null).Max + room.get_BoundingBox(null).Min) / 2;
            return point;
        }
    }
}
