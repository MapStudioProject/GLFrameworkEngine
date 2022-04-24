using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class DrawableSolidFloor : RenderMesh<VertexPositionTexCoord>
    {
        /// <summary>
        /// Toggles to display a solid floor or not.
        /// </summary>
        public static bool Display = true;

        /// <summary>
        /// The texture to display on the solid floor.
        /// </summary>
        public GLTexture Texture = null;

        private StandardMaterial Material = new StandardMaterial();

        //scale of the floor
        private const float SCALE = 200;

        public DrawableSolidFloor() : base(Vertices, PrimitiveType.TriangleStrip)
        {
        }

        static VertexPositionTexCoord[] Vertices => new VertexPositionTexCoord[]
        {
               new VertexPositionTexCoord(new Vector3(1.0f, 0, 1.0f) * SCALE, new Vector2(0, 1)),
               new VertexPositionTexCoord(new Vector3(1.0f, 0,-1.0f) * SCALE, new Vector2(0, 0)),
               new VertexPositionTexCoord(new Vector3(-1.0f,0, 1.0f) * SCALE, new Vector2(1, 1)),
               new VertexPositionTexCoord(new Vector3(-1.0f,0,-1.0f) * SCALE, new Vector2(1, 0)),
        };

        public void SetImage(string filePath) {
            if (System.IO.File.Exists(filePath))
                Texture = GLTexture2D.FromBitmap(new Bitmap(filePath));
        }

        public void Draw(GLContext control, Pass pass)
        {
            if (pass != Pass.OPAQUE || !Display)
                return;

            if (Texture != null)
                Material.DiffuseTextureID = Texture.ID;

            Material.Render(control);
            this.Draw(control);

            GL.UseProgram(0);
        }
    }
}