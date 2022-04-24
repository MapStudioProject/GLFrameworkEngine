using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class CubeRender : RenderMesh<VertexPositionNormal>, IDrawable
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// The material of the cube.
        /// </summary>
        private readonly StandardMaterial Material = new StandardMaterial();

        /// <summary>
        /// Toggles visibility of the cursor.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        GLTexture2D TexturedCube = null;

        public CubeRender() : base(GetCubeVertices(1), PrimitiveType.Triangles)
        {
            TexturedCube = GLTexture2D.FromBitmap(Properties.Resources.DefaultTexture);

            //Example of updating vertex data
            this.UpdateVertexData(GetCubeVertices(10));
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE) //Only want to draw in opaque pass
                return;

            //Assigning textures to standard material
            Material.DiffuseTextureID = TexturedCube.ID;
            Material.ModelMatrix = Transform.TransformMatrix;
            Material.Render(context);
            Draw(context);
        }

        public static VertexPositionNormal[] GetCubeVertices(float size)
        {
            VertexPositionNormal[] vertices = new VertexPositionNormal[8];
            vertices[0] = new VertexPositionNormal(new Vector3(-1, -1, 1), new Vector3(-1, -1, 1)); //Bottom Left
            vertices[1] = new VertexPositionNormal(new Vector3(1, -1, 1), new Vector3(1, -1, 1)); //Bottom Right
            vertices[2] = new VertexPositionNormal(new Vector3(1, 1, 1), new Vector3(1, 1, 1)); //Top Right
            vertices[3] = new VertexPositionNormal(new Vector3(-1, 1, 1), new Vector3(-1, 1, 1)); //Top Left
            vertices[4] = new VertexPositionNormal(new Vector3(-1, -1, -1), new Vector3(-1, -1, -1)); //Bottom Left -Z
            vertices[5] = new VertexPositionNormal(new Vector3(1, -1, -1), new Vector3(1, -1, -1)); //Bottom Right -Z
            vertices[6] = new VertexPositionNormal(new Vector3(1, 1, -1), new Vector3(1, 1, -1)); //Top Right -Z
            vertices[7] = new VertexPositionNormal(new Vector3(-1, 1, -1), new Vector3(-1, 1, -1)); //Top Left -Z
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position *= size;
            return vertices;
        }
    }
}
