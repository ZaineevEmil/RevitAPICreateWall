using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICreateWall
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            List<Wall> walls = new List<Wall>();

            Level level1 = GetLevel(levels, "Уровень 1");
            Level level2 = GetLevel(levels, "Уровень 2");

            // ввести длину и ширину стен в мм
            double length = 10000;
            double width = 5000;
            List<Line> linesWalls = CreateLinesOfWalls(length, width);


            using (var ts = new Transaction(doc, "Создание стен"))
            {
                ts.Start();
                foreach (Line line in linesWalls)
                {
                    CreateWalls(walls, doc, line, level1, level2);
                }
                ts.Commit();
            }
            return Result.Succeeded;
        }




        public Level GetLevel(List<Level> levels, string nameLevel)
        {
            Level level1 = levels
                .Where(x => x.Name.Equals(nameLevel))
                .FirstOrDefault();
            return level1;
        }


        public List<Line> CreateLinesOfWalls(double length, double width)
        {
            double lengthInch = UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Millimeters);
            double widthInch = UnitUtils.ConvertToInternalUnits(width, UnitTypeId.Millimeters);
            double dx = lengthInch / 2;
            double dy = widthInch / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Line> linesWalls = new List<Line>();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                linesWalls.Add(line);
            }
            return linesWalls;
        }


        public List<Wall> CreateWalls(List<Wall> walls, Document doc, Line line, Level level1, Level level2)
        {
            Wall wall = Wall.Create(doc, line, level1.Id, false);
            wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            walls.Add(wall);
            return walls;
        }


    }
}
