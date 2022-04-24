using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;

namespace GLFrameworkEngine.FileData
{
    /// <summary>
    /// Curve data that can be serialized as json into a render curve object.
    /// </summary>
    public class CurveFileData
    {
        public bool IsBezier { get; set; }
        public bool Loop { get; set; }

        public List<Point> Points = new List<Point>();

        public static CurveFileData Load(string filePath) {
            return JsonConvert.DeserializeObject<CurveFileData>(File.ReadAllText(filePath));
        }

        public RenderablePath CreateRenderer()
        {
            RenderablePath path = new RenderablePath();
            ImportRenderer(path);
            return path;
        }

        public void ImportRenderer(RenderablePath path)
        {
            path.PathPoints.Clear();
            path.Loop = this.Loop;
            if (this.IsBezier)
                path.InterpolationMode = RenderablePath.Interpolation.Bezier;
            foreach (var pt in Points)
            {
                var pathPoint = new RenderablePathPoint(path, new OpenTK.Vector3());
                path.AddPoint(pathPoint);

                pathPoint.Transform.Position = new OpenTK.Vector3(
                    pt.Translation.X,
                    pt.Translation.Y,
                    pt.Translation.Z);
                pathPoint.ControlPoint1.Transform.Position = new OpenTK.Vector3(
                    pt.ControlPoint1.X,
                    pt.ControlPoint1.Y,
                    pt.ControlPoint1.Z);
                pathPoint.ControlPoint2.Transform.Position = new OpenTK.Vector3(
                    pt.ControlPoint2.X,
                    pt.ControlPoint2.Y,
                    pt.ControlPoint2.Z);
                pathPoint.UpdateMatrices();
            }
        }

        public void ExportRenderer(RenderablePath path)
        {
            this.Loop = path.Loop;
            this.IsBezier = (path.InterpolationMode == RenderablePath.Interpolation.Bezier);

            Points.Clear();
            foreach (var pathPoint in path.PathPoints)
            {
                var pt = new Point();
                Points.Add(pt);

                pt.Translation = new Vector3(
                    pathPoint.Transform.Position.X,
                    pathPoint.Transform.Position.Y,
                    pathPoint.Transform.Position.Z);
                pt.ControlPoint1 = new Vector3(
                    pathPoint.ControlPoint1.Transform.Position.X,
                    pathPoint.ControlPoint1.Transform.Position.Y,
                    pathPoint.ControlPoint1.Transform.Position.Z);
                pt.ControlPoint2 = new Vector3(
                    pathPoint.ControlPoint2.Transform.Position.X,
                    pathPoint.ControlPoint2.Transform.Position.Y,
                    pathPoint.ControlPoint2.Transform.Position.Z);
            }
        }

        public void Save(string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class Point
    {
        public Vector3 Translation { get; set; }
        public Vector3 ControlPoint1 { get; set; }
        public Vector3 ControlPoint2 { get; set; }
    }
}
