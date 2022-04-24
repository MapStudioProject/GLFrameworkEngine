using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using GLFrameworkEngine.FileData;

namespace GLFrameworkEngine
{
    public partial class RenderablePath : IDrawable, IEditModeObject, IColorPickable, ITransformableObject
    {
        public void CreateFromFile(string filePath)
        {
            var fileData = CurveFileData.Load(filePath);
            fileData.ImportRenderer(this);
        }

        public void ExportAsFile(string filePath)
        {
            var fileData = new CurveFileData();
            fileData.ExportRenderer(this);
            fileData.Save(filePath);
        }

        /// <summary>
        /// Creates a standard bezier curve using 2 points with handles.
        /// </summary>
        public void CreateLinearStandard(float length = 1)
        {
            this.InterpolationMode = Interpolation.Linear;
            this.Loop = false;

            float halfLength = length / 2;

            PathPoints.Clear();
            AddPoint(this.CreatePoint(new Vector3(-halfLength, 0, 0)));
            AddPoint(this.CreatePoint(new Vector3(halfLength, 0, 0)));

            PathPoints[0].AddChild(PathPoints[1]);
        }

        /// <summary>
        /// Creates a standard bezier curve using 2 points with handles.
        /// </summary>
        public void CreateBezierStandard(float length = 1)
        {
            this.InterpolationMode = Interpolation.Bezier;
            this.Loop = false;

            float halfLength = length / 2;

            PathPoints.Clear();
            AddPoint(CreateBezierPoint(new Vector3(-length, 0, 0), new Vector3(-halfLength, 0, halfLength), new Vector3(halfLength, 0, -halfLength)));
            AddPoint(CreateBezierPoint(new Vector3(length, 0, 0), new Vector3(-length, 0, 0), new Vector3(length, 0, -0)));
        }

        /// <summary>
        /// Creates a looped circle of a bezier curve.
        /// </summary>
        public void CreateBezierCircle(float length = 1.0f)
        {
            this.InterpolationMode = Interpolation.Bezier;
            this.Loop = true;

            float handleLength = length * 0.5f;

            //Make 4 points on each side
            //Create handles on each side
            PathPoints.Clear();
            AddPoint(CreateBezierPoint(new Vector3(-length, 0, 0), new Vector3(0, 0, handleLength), new Vector3(0, 0, -handleLength)));
            AddPoint(CreateBezierPoint(new Vector3(0, 0, -length), new Vector3(-handleLength, 0, 0), new Vector3(handleLength, 0, 0)));
            AddPoint(CreateBezierPoint(new Vector3(length, 0, 0), new Vector3(0, 0, -handleLength), new Vector3(0, 0, handleLength)));
            AddPoint(CreateBezierPoint(new Vector3(0, 0, length), new Vector3(handleLength, 0, 0), new Vector3(-handleLength, 0, 0)));
        }

        private RenderablePathPoint CreateBezierPoint(Vector3 pos, Vector3 handle1, Vector3 handle2)
        {
            var point = this.CreatePoint(pos);
            point.ControlPoint1.Transform.Position = pos + handle1;
            point.ControlPoint2.Transform.Position = pos + handle2;
            point.UpdateMatrices();
            return point;
        }
    }
}
