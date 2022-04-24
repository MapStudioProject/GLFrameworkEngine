using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class ColorPicker
    {
        private Framebuffer pickingBuffer;

        public Dictionary<uint, ITransformableObject> ColorPassIDs = new Dictionary<uint, ITransformableObject>();

        public bool EnablePicking = true;

        public float NormalizedPickingDepth;

        private float Depth;

        private int pickingIndex = 1;

        public GLTexture2D GetDebugPickingDisplay() {
            if (pickingBuffer == null) return null;

           return (GLTexture2D)pickingBuffer.Attachments[0];
        }

        public SelectionMode PickingMode = SelectionMode.Object;

        public enum SelectionMode
        {
            Object,
            Model,
            Mesh,
            Material,
            Face,
        }

        public void SetPickingColorFaces(List<ITransformableObject> pickables, ShaderProgram shader)
        {
            shader.SetInt("pickFace", 1);
            shader.SetInt("pickedIndex", pickingIndex);

            for (int i = 0; i < pickables.Count; i++)
            {
                var color = new Vector4(
                 ((pickingIndex >> 16) & 0xFF),
                 ((pickingIndex >> 8) & 0xFF),
                 (pickingIndex & 0xFF),
                 ((pickingIndex++ >> 24) & 0xFF)
                 );

                var key = BitConverter.ToUInt32(new byte[]{
                    (byte)color.X, (byte)color.Y,
                    (byte)color.Z, (byte)color.W
                }, 0);
                ColorPassIDs.Add(key, pickables[i]);
            }
        }

        public void SetPickingColor(ITransformableObject pickable, ShaderProgram shader)
        {
            var color = SetPickingColor(pickable);
            shader.SetVector4("color", color);
            shader.SetInt("pickFace", 0);
        }

        public Vector4 SetPickingColor(ITransformableObject pickable)
        {
            var color = new Vector4(
               ((pickingIndex >> 16) & 0xFF),
               ((pickingIndex >> 8) & 0xFF),
               (pickingIndex & 0xFF),
               ((pickingIndex++ >> 24) & 0xFF)
               );

            var key = BitConverter.ToUInt32(new byte[]{
                (byte)color.X, (byte)color.Y,
                (byte)color.Z, (byte)color.W
            }, 0);

            ColorPassIDs.Add(key, pickable);
            return color / 255.0f;
        }

        private void Init(int width, int height)
        {
            if (pickingBuffer == null)
                pickingBuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba, 1);

            if (pickingBuffer.Width != width || pickingBuffer.Height != height)
                pickingBuffer.Resize(width, height);
        }

        public ITransformableObject FindPickableAtPosition(GLContext context, List<ITransformableObject> drawables, Vector2 position)
        {
            if (drawables == null) return null;

            Prepare(context.Width, context.Height);

            //Draw the pickable objects. Drawn IDs will be passed into ColorPassIDs
            foreach (var drawable in drawables)
            {
                if (drawable is IColorPickable)
                    ((IColorPickable)drawable).DrawColorPicking(context);
            }

            uint pickingColor = 0;

            GL.Flush();
            GL.ReadPixels((int)position.X, (int)position.Y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pickingColor);

            pickingBuffer.Unbind();

            foreach (var drawable in drawables)
                drawable.IsHovered = false;

            return SearchPickedColor(pickingColor);
        }

        public List<ITransformableObject> FindPickablesAtRegion(GLContext context, List<IColorPickable> drawables, Vector2 startPoint, Vector2 endPoint)
        {
            Prepare(context.Width, context.Height);

            GL.PushAttrib(AttribMask.AllAttribBits);

            //Draw the pickable objects. Drawn IDs will be passed into ColorPassIDs
            foreach (var drawable in drawables)
                drawable.DrawColorPicking(context);

            GL.PopAttrib();

            GL.Flush();

            int width  = (int)(endPoint.X - startPoint.X);
            int height = (int)(endPoint.Y - startPoint.Y);

            uint[] pickingColor = new uint[width * height];

            GL.UseProgram(0);
            GL.ReadPixels((int)startPoint.X, (int)startPoint.Y, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pickingColor);

            pickingBuffer.Unbind();

            foreach (ITransformableObject drawable in drawables)
                drawable.IsHovered = false;

            List<ITransformableObject> selected = new List<ITransformableObject>();
            for (int i = 0; i < pickingColor.Length; i++)
            {
                var pickable = SearchPickedColor(pickingColor[i]);
                if (pickable != null)
                    selected.Add(pickable);
            }
            return selected;
        }

        private void Prepare(int width, int height)
        {
            Init(width, height);

            pickingBuffer.Bind();
            GL.Viewport(0, 0, width, height);
            GL.ClearColor(1, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ColorPassIDs.Clear();
            pickingIndex = 1;
        }

        public void UpdatePickingDepth(GLContext context, Vector2 position)
        {
            var camera = context.Camera;

            GL.ReadPixels((int)position.X, (int)position.Y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref Depth);

            //Get normalized depth for z depth
            if (Depth == 1.0f) //1.0 max depth value used.
                NormalizedPickingDepth = camera.ZFar;
            else
                NormalizedPickingDepth = -(camera.ZFar * camera.ZNear / (Depth * (camera.ZFar - camera.ZNear) - camera.ZFar));

            camera.Depth = NormalizedPickingDepth;
        }

        /// <summary>
        /// Searches and returns the object that has a color id match from the picking color buffer.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public ITransformableObject SearchPickedColor(uint color)
        {
            if (ColorPassIDs.ContainsKey(color))
                return ColorPassIDs[color];

            return null;
        }
    }
}
