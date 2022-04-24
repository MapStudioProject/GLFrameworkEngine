using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class SelectionCircle 
    {
        static RenderMesh<Vector2> Mesh;

        static void Init()
        {
            if (Mesh != null)
                return;

            Mesh = new RenderMesh<Vector2>(GetVertices(60), PrimitiveType.Lines);
        }

        static Vector2[] GetVertices(int length = 60)
        {
            Vector2[] data = new Vector2[length];
            for (int i = 0; i < length; i++)
            {
                double angle = 2 * Math.PI * i / length;
                data[i] = new Vector2(
                    MathF.Cos((float)angle),
                    MathF.Sin((float)angle));
            }
            return data;
        }

        private Vector2 position;
        private float radius = 0.5f;

        public void Start(GLContext context, float x, float y) {
            position = new Vector2(x, y);
        }

        public void Resize(float delta)
        {
            radius -= delta * 0.05f;
            radius = MathF.Max(radius, 0.05F);
        }

        public void Render(GLContext context, float x, float y) {
            position = new Vector2(x, y);

            //Display the circle
            Draw(context, context.NormalizeMouseCoords(position), radius);
        }

        public void Apply(GLContext context, float x, float y, bool select) {
            position = new Vector2(x, y);

            //Check if the position of objects is inside the selection
            var objects = context.Scene.GetSelectableObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].CanSelect)
                    continue;

                var sceenCoord = context.WorldToScreen(objects[i].Transform.Position);
                //Use normalized coordinates for comparing the sphere size
                if (PointInsideSphere(
                    context.NormalizeMouseCoords(sceenCoord),
                    context.NormalizeMouseCoords(position), radius))
                {
                    objects[i].IsSelected = select;
                }
            }

            context.Scene.OnSelectionChanged(context);
        }

        bool PointInsideSphere(Vector2 point, Vector2 circlePoint, float radius)
        {
            float dist = Vector2.Distance(point, circlePoint);
            if (dist <= (radius * radius))
                return true;

            return false;
        }

        static void Draw(GLContext context, Vector2 position, float scale)
        {
            Init();

            Vector2 scaleOutput = new Vector2(
                370.0f / context.Width,
                370.0f / context.Height) * scale;

            var mdlMtx = Matrix4.CreateScale(scaleOutput.X, scaleOutput.Y, 1.0f) * Matrix4.CreateTranslation(position.X, position.Y, 0);

            SelectionRenderer.DrawFilledMask(context, mdlMtx, Mesh, PrimitiveType.TriangleFan);
            SelectionRenderer.DrawDashedOutline(context, mdlMtx, Mesh, PrimitiveType.LineLoop);
        }
    }
}
