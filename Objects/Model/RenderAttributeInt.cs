using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RenderAttributeInt : RenderAttribute
    {
        public RenderAttributeInt(string attributeName, VertexAttribPointerType attributeFormat) 
            : base(attributeName, attributeFormat)
        {

        }

        public RenderAttributeInt(string attributeName, VertexAttribPointerType attributeFormat, int offset)
                : base(attributeName, attributeFormat, offset)
        {
        }

        public RenderAttributeInt(int attributeLocation, VertexAttribPointerType attributeFormat, int offset)
                : base(attributeLocation, attributeFormat, offset)
        {
        }

        public override void SetAttribute(int index, int stride)
        {
            GL.VertexAttribIPointer(index, ElementCount, (VertexAttribIntegerType)Type, stride, new System.IntPtr(Offset.Value));
        }
    }
}
