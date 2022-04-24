using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class ObjectLinkDrawer
    {
        RenderMesh<LinkVertex> LineRender;
        StandardMaterial LineMaterial = new StandardMaterial();

        //View colors
        Vector4 SrcColor_View = new Vector4(0, 0, 1, 1);
        Vector4 DestColor_View = new Vector4(1, 1, 0, 1);
        //Editing colors
        Vector4 SrcColor_Edit = new Vector4(0, 1, 0, 1);
        Vector4 DestColor_Edit = new Vector4(1, 0, 0, 1);

        const float LINE_WIDTH = 5;

        public void DrawPicking(GLContext context)
        {
          
        }

        public void Draw(GLContext context)
        {
            if (LineRender == null)
                LineRender = new RenderMesh<LinkVertex>(new LinkVertex[0], PrimitiveType.LineLoop);

            LineMaterial.hasVertexColors = true;
            LineMaterial.Render(context);

            var objects = context.Scene.GetSelectableObjects();
            foreach (var obj in objects) {
                //Connect references from IObjectLink types
                if (obj is IObjectLink)
                    DrawLinks(context, (IObjectLink)obj);
            }
        }

        private void DrawLinks(GLContext context, IObjectLink obj)
        {
            if (obj is IDrawable && !((IDrawable)obj).IsVisible)
                return;

            //Connect from the current source of the object. 
            var sourcePos = ((ITransformableObject)obj).Transform.Position;
            List<LinkVertex> points = new List<LinkVertex>();

            //Connect to each link reference
            foreach (ITransformableObject linkedObj in obj.DestObjectLinks) {
                if (((ITransformableObject)obj).IsSelected || linkedObj.IsSelected || context.LinkingTools.DisplayAllLinks)
                {
                    var destPos = linkedObj.Transform.Position;
                    //2 types of colors, edit mode and link view
                    bool editMode = false;
                    if (editMode) {
                        points.Add(new LinkVertex(sourcePos, SrcColor_Edit));
                        points.Add(new LinkVertex(destPos, DestColor_Edit));
                    }
                    else {
                        points.Add(new LinkVertex(sourcePos, SrcColor_View));
                        points.Add(new LinkVertex(destPos, DestColor_View));
                    }
                }
            }

            if (points.Count > 0) {
                GL.LineWidth(LINE_WIDTH);
                LineRender.UpdateVertexData(points.ToArray(), BufferUsageHint.DynamicDraw);
                LineRender.Draw(context);
                GL.LineWidth(1);
            }
        }

        class LinkDrawer
        {
            public Vector3 SourcePos;
            public Vector3 DestPos;

            public void Draw()
            {

            }
        }

        struct LinkVertex
        {
            [RenderAttribute("vPosition", VertexAttribPointerType.Float, 0)]
            public Vector3 Position;

            [RenderAttribute("vColor", VertexAttribPointerType.Float, 12)]
            public Vector4 vColor;

            public LinkVertex(Vector3 position, Vector4 color)
            {
                Position = position;
                vColor = color;
            }
        }
    }
}
