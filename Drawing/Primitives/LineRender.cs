using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class LineRender : RenderMesh<LineRender.LineVertex>
    {
        public struct LineVertex
        {
            [RenderAttribute(GLConstants.VPosition, VertexAttribPointerType.Float, 0)]
            public Vector3 Position;

            [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 12)]
            public Vector4 Color;

            public LineVertex(Vector3 position, Vector4 color) {
                Position = position;
                Color = color;
            }

            public LineVertex(Vector2 position, Vector4 color) {
                Position = new Vector3(position.X, position.Y, 0);
                Color = color;
            }
        }

        private int length = 0;

        public LineRender(PrimitiveType type = PrimitiveType.Lines) : base(new LineVertex[0], type)
        {
        }

        public void Draw(Vector3 start, Vector3 end, Vector4 color, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(start, end, color);

            this.Draw(GLContext.ActiveContext);
        }

        public void Draw(Vector2 start, Vector2 end, Vector4 color, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(start, end, color);

            this.Draw(GLContext.ActiveContext);
        }

        public void Draw(List<Vector2> points, bool forceUpdate = false) {
            if (length == 0 || forceUpdate)
                UpdateVertexData(points, new List<Vector4>() { Vector4.One });

            this.Draw(GLContext.ActiveContext);
        }

        public void Draw(GLContext context, List<Vector3> points, bool forceUpdate = false) {
            Draw(context, points, new List<Vector4>() { Vector4.One }, forceUpdate);
        }

        public void Draw(GLContext context, List<Vector3> points, List<Vector4> colors, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(points, colors);

            this.Draw(context);
        }


        void UpdateVertexData(Vector2 start, Vector2 end, Vector4 color) {
            UpdateVertexData(new Vector3(start.X, start.Y, 0), new Vector3(end.X, end.Y, 0), color);
        }

        void UpdateVertexData(Vector3 start, Vector3 end, Vector4 color) {
            UpdateVertexData(new List<Vector3>() { start, end }, new List<Vector4>() { color, color });
        }

        void UpdateVertexData<T>(List<T> points, List<Vector4> colors) where T : struct
        {
            List<LineVertex> list = new List<LineVertex>();
            for (int i = 0; i < points.Count; i++)
            {
                var pt = (object)points[i];

                Vector4 color = new Vector4(1);
                if (colors.Count > i)
                    color = colors[i];
                if (points[i] is Vector3)
                    list.Add(new LineVertex((Vector3)pt, color));
                else if (points[i] is Vector2)
                    list.Add(new LineVertex((Vector2)pt, color));
            }
            LineVertex[] data = list.ToArray();
            this.UpdateVertexData(data);

            length = data.Length;
        }
    }
}
