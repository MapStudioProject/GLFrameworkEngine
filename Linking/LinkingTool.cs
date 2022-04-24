using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class LinkingTool
    {
        public bool DisplayAllLinks = false;

        public bool IsActive => SourceObjectLink != null;

        bool CanEdit = false;

        RenderMesh<LinkVertex> LinkRender;

        IObjectLink SourceObjectLink;
        IObjectLink DestObjectLink;
        IObjectLink ConnectingLink;

        enum ConnectionType
        {
            None,
            ToSource,
            ToDest,
        }

        ConnectionType ConnectionAction;

        public void OnMouseDown(GLContext context, MouseEventInfo e)
        {
            if (IsActive || !CanEdit)
                return;

            //Create new connection
            if (KeyEventInfo.State.KeyCtrl)
            {
                //Check for a picked model during a linkable operation
                Vector2 position = new Vector2(e.Position.X, context.Height - e.Position.Y);
                var pickable = context.Scene.FindPickableAtPosition(context, position);
                //No linking operation if no pickable is found
                if (pickable == null)
                    return;

                //Start a link operation
                if (pickable is IObjectLink)
                    SetLinkSource(context, pickable);
            }
        }

        public void OnMouseMove(GLContext context, MouseEventInfo e)
        {
            if (!IsActive)
                return;

            //Check for a picked model during a linkable operation
            Vector2 position = new Vector2(e.Position.X, context.Height - e.Position.Y);
            ConnectingLink = context.Scene.FindPickableAtPosition(context, position) as IObjectLink;
        }

        public void OnMouseUp(GLContext context, MouseEventInfo e)
        {
            if (!IsActive)
                return;

            //Apply the link when the mouse has been released
            if (ConnectingLink != null)
                LinkObject(SourceObjectLink, ConnectingLink);
            else
                UnlinkObject(SourceObjectLink, DestObjectLink);

            SourceObjectLink = null;
            DestObjectLink = null;
            ConnectingLink = null;
        }

        private void LinkObject(IObjectLink objectLink, IObjectLink dest)
        {
            if (!objectLink.DestObjectLinks.Contains((ITransformableObject)dest))
                objectLink.DestObjectLinks.Add((ITransformableObject)dest);

            if (!dest.SourceObjectLinks.Contains((ITransformableObject)objectLink))
                dest.SourceObjectLinks.Add((ITransformableObject)objectLink);
        }

        private void UnlinkObject(IObjectLink objectLink, IObjectLink dest)
        {
            if (dest == null)
                return;

            if (objectLink.DestObjectLinks.Contains((ITransformableObject)dest))
                objectLink.DestObjectLinks.Remove((ITransformableObject)dest);

            if (dest.SourceObjectLinks.Contains((ITransformableObject)objectLink))
                dest.SourceObjectLinks.Remove((ITransformableObject)objectLink);
        }

        public void SetLinkSource(GLContext context, ITransformableObject linkableObject)
        {
            if (!(linkableObject is IObjectLink)) //Object not a linkable type
                return;

            //Set the source link from a linkable object.
            var link = (IObjectLink)linkableObject;
            if (link.SourceObjectLinks.Count > 0)
            {
                SourceObjectLink = (IObjectLink)link.SourceObjectLinks.FirstOrDefault();
                DestObjectLink = link;
                ConnectingLink = link;
                ConnectionAction = ConnectionType.ToSource;
            }
            else
            {
                ConnectionAction = ConnectionType.ToDest;
                SourceObjectLink = (IObjectLink)linkableObject;
            }
        }

        public void Render(GLContext context, float x, float y)
        {
            if (!IsActive)
                return;

            //Don't draw a link connector until a source is set
            if (SourceObjectLink == null)
            {
                foreach (var ob in context.Scene.Objects) {
                    if (ob is IObjectLink) {
                        var link = (IObjectLink)ob;
                        foreach (var dest in link.DestObjectLinks)
                        {
                            Vector3 positon1 = ((ITransformableObject)link).Transform.Position;
                            Vector3 positon2 = ((ITransformableObject)dest).Transform.Position;
                            Vector2 srcPos1 = context.WorldToScreen(positon1);
                            Vector2 destPos2 = context.WorldToScreen(positon2);

                            DrawConnection(context, srcPos1, destPos2, true);
                        }
                    }
                }
                return;
            }

            //Object links are always transformable with a position. Get the world pos
            Vector3 positon = ((ITransformableObject)SourceObjectLink).Transform.Position;
            Vector2 srcPos = context.WorldToScreen(positon);

            //Draw the current mouse placement
            DrawConnection(context, srcPos, new Vector2(x, y), ConnectingLink != null);
        }

        private void DrawConnection(GLContext context, Vector2 srcPos, Vector2 destPos, bool hasDest)
        {
            var normalized1 = context.NormalizeMouseCoords(srcPos);
            var normalized2 = context.NormalizeMouseCoords(destPos);

            if (LinkRender == null)
                LinkRender = new RenderMesh<LinkVertex>(new LinkVertex[0], PrimitiveType.LineLoop);

            LinkRender.UpdateVertexData(new LinkVertex[] {
                 new LinkVertex(normalized1, new Vector4(0, 1, 0, 1)),
                 new LinkVertex(normalized2, new Vector4(1, 0, 0, 1)),
            });

            //Connection render
            GL.LineWidth(5);

            var mat = new LinkRenderMaterial();
            if (hasDest)
                mat.UseVertexColors = true;
            else
            {
                mat.UseVertexColors = false;
                mat.Color = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
            }
            mat.Render(context);
            LinkRender.UpdatePrimitiveType(PrimitiveType.LineLoop);
            LinkRender.Draw(context);

            GL.LineWidth(1);

            GL.PointSize(10);

            LinkRender.UpdatePrimitiveType(PrimitiveType.Points);
            LinkRender.Draw(context);

            GL.PointSize(1);
        }
    }

    struct LinkVertex 
    {
        [RenderAttribute("vPosition", VertexAttribPointerType.Float, 0)]
        public Vector2 Position;

        [RenderAttribute("vColor", VertexAttribPointerType.Float, 8)]
        public Vector4 vColor;

        public LinkVertex(Vector2 position, Vector4 color)
        {
            Position = position;
            vColor = color;
        }
    }

    class LinkRenderMaterial
    {
        public Vector2 Position = Vector2.Zero;
        public float Scale = 1.0f;
        public bool UseVertexColors = false;
        public Vector4 Color = Vector4.One;

        public void Render(GLContext context)
        {
            var shader = GlobalShaders.GetShader("LINKING");
            context.CurrentShader = shader;

            var vp = Matrix4.Identity;
            shader.SetMatrix4x4(GLConstants.ViewProjMatrix, ref vp);

            shader.SetFloat("Time", 0);
            shader.SetFloat("scale", Scale);
            shader.SetVector2("pos", Position);
            shader.SetVector4("color", Color);

            shader.SetBoolToInt("useVertexColors", UseVertexColors);
        }
    }
}
