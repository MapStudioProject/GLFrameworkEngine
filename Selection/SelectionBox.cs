using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class SelectionBox
    {
        static RenderMesh<Vector2> Mesh;

        static void Init()
        {
            if (Mesh == null)
                Mesh = new RenderMesh<Vector2>(new Vector2[6], PrimitiveType.LineLoop);
        }

        public Vector2 MinPoint
        {
            get
            {
                return new Vector2(
               MathF.Min(startPoint.X, endPoint.X),
               MathF.Min(startPoint.Y, endPoint.Y));
            }
        }

        public Vector2 MaxPoint
        {
            get
            {
                return new Vector2(
               MathF.Max(startPoint.X, endPoint.X),
               MathF.Max(startPoint.Y, endPoint.Y));
            }
        }

        private Vector2 startPoint;
        private Vector2 endPoint;

        List<ITransformableObject> selectedObjects = new List<ITransformableObject>();

        private SELECT_ACTION SelectAction = SELECT_ACTION.NONE;

        public void StartSelection(GLContext context, float x, float y) {
            startPoint = new Vector2(x, y);
            SelectAction = SELECT_ACTION.SELECT;
        }

        public void StartDeselection(GLContext context, float x, float y) {
            startPoint = new Vector2(x, y);
            SelectAction = SELECT_ACTION.DESELECT;
        }

        public void Render(GLContext context, float x, float y) {
            Init();

            if (SelectAction == SELECT_ACTION.NONE)
            {
                //Draw inital point for marking a starting point
                DrawSelectionMarker(context, new Vector2(x, y));
                return;
            }

            endPoint = new Vector2(x, y);

            //Display the box. Normalize into gl coordinate system
            Vector2 screenPos1 = context.NormalizeMouseCoords(MinPoint);
            Vector2 screenPos2 = context.NormalizeMouseCoords(MaxPoint);

            Draw(context, screenPos1, screenPos2);

            if (SelectAction != SELECT_ACTION.NONE)
                Apply(context, endPoint.X, endPoint.Y);
        }

        public void Apply(GLContext context, float x, float y) {
            endPoint = new Vector2(x, y);

            //Check if the position of objects is inside the selection
            var objects = context.Scene.GetSelectableObjects();
            //Set the default setting before selection
            foreach (var ob in selectedObjects)
                ob.IsSelected = SelectAction != SELECT_ACTION.SELECT;

            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].CanSelect)
                    continue;

                var screenPoint = context.WorldToScreen(objects[i].Transform.Position);
                if (screenPoint.X < MaxPoint.X && screenPoint.X > MinPoint.X &&
                    screenPoint.Y < MaxPoint.Y && screenPoint.Y > MinPoint.Y)
                {
                    if (SelectAction == SELECT_ACTION.SELECT)
                        objects[i].IsSelected = true;
                    else
                        objects[i].IsSelected = false;

                    //Add to a list to keep track of the current selection
                    if (!selectedObjects.Contains(objects[i]))
                        selectedObjects.Add(objects[i]);
                }
            }
            context.Scene.OnSelectionChanged(context);
        }

        static void DrawSelectionMarker(GLContext context, Vector2 point)
        {
            Vector2 screenPos = context.NormalizeMouseCoords(point);

            Mesh.UpdateVertexData(new Vector2[]
            {
                new Vector2(-context.Width + screenPos.X, screenPos.Y),
                new Vector2(context.Width + screenPos.X, screenPos.Y),

                new Vector2(screenPos.X, -context.Height +  screenPos.Y),
                new Vector2(screenPos.X, context.Height + screenPos.Y),
            });

            SelectionRenderer.DrawDashedOutline(context, Matrix4.Identity, Mesh, PrimitiveType.Lines);
        }

        static void Draw(GLContext context, Vector2 min, Vector2 max)
        {
            Mesh.UpdateVertexData(new Vector2[]
            {
                new Vector2(min.X, max.Y),
                new Vector2(max.X, max.Y),
                new Vector2(max.X, min.Y),
                new Vector2(min.X, min.Y),
                new Vector2(min.X, max.Y),
            });

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            var mdlMtx = Matrix4.Identity;

            SelectionRenderer.DrawFilledMask(context, mdlMtx, Mesh, PrimitiveType.TriangleStrip);
            SelectionRenderer.DrawDashedOutline(context, mdlMtx, Mesh, PrimitiveType.LineLoop);
        }

        enum SELECT_ACTION
        {
            NONE,
            SELECT,
            DESELECT,
        }
    }
}
