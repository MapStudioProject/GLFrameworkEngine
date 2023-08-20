using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class ConeRenderer : RenderMesh<VertexPositionNormal>
    {
        public ConeRenderer(float radiusBottom, float radiusTop, float height, float subdiv = 32, float startHeight = 0)
            : base(DrawingHelper.GetConeVertices(radiusBottom, radiusTop, height, subdiv, startHeight),
                  PrimitiveType.Triangles)
        {

        }
    }
}
