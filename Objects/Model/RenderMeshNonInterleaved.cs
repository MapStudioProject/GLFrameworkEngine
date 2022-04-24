using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class RenderMeshNonInterleaved : RenderMeshBase
    {
        private VertexBufferObject vbo;
        private int? indexBuffer;
        private bool vboInit = false;

        public RenderMeshNonInterleaved(int[] indices, PrimitiveType type) : base(type)
        {
            if (indices != null)
                SetIndices(indices);
        }

        public RenderMeshNonInterleaved(uint[] indices, PrimitiveType type) : base(type)
        {
            if (indices != null)
                SetIndices(indices);
        }

        public void SetIndices<T>(T[] indices) where T : struct
        {
            indexBufferData = new BufferObject(BufferTarget.ElementArrayBuffer);
            indexBufferData.SetData(indices, BufferUsageHint.StaticDraw);
            indexBuffer = indexBufferData.ID;

            DrawCount = indices.Length;
        }

        protected override void BindVAO()
        {
            InitVertexBufferObject();
            vbo.Use();
        }

        protected override void PrepareAttributes(ShaderProgram shader)
        {
            InitVertexBufferObject();
            vbo.Enable(shader);
        }

        public void InitVertexBufferObject()
        {
            if (vboInit)
                return;

            var ids = buffers.Select(x => x.ID).ToArray();

            //Setup the buffer object for loading
            vbo = new VertexBufferObject(ids, indexBuffer);
            foreach (var att in attributes)
            {
                //Set the vertex stride
                int vertexStride = attributes.Sum(x => x.BufferIndex == att.BufferIndex ? x.Size : 0);

                if (!string.IsNullOrEmpty(att.Name))
                    vbo.AddAttribute(att.Name, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value, att.BufferIndex);
                else
                    vbo.AddAttribute(att.Location, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value, att.BufferIndex);
            }
            vbo.Initialize();
            vboInit = true;
        }

        public void ClearAttributes()
        {
            vbo.Dispose();
            vboInit = false;

            //Also reset the buffer data
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                    buffer?.Dispose();
            }
            //Empty the lists
            attributes = new RenderAttribute[0];
            buffers = new BufferObject[0];
        }

        public void AddAttributes(RenderAttribute[] attributes)
        {
            this.attributes = attributes.ToArray();

            int maxBufferCount = attributes.Max(x => x.BufferIndex) + 1;
            //Resize the buffer list accordingly. 
            if (maxBufferCount != buffers.Length)
                buffers = new BufferObject[maxBufferCount];
        }

        public void AddAttribute(RenderAttribute attribute)
        {
            var attributeList = this.attributes.ToList();
            attributeList.Add(attribute);

            this.attributes = attributeList.ToArray();

            int maxBufferCount = attributes.Max(x => x.BufferIndex) + 1;
            //Resize the buffer list accordingly. 
            if (maxBufferCount != buffers.Length)
                buffers = new BufferObject[maxBufferCount];
        }

        public void SetData<T>(T[] data, int index) where T : struct {
            if (buffers[index] == null)
                buffers[index] = new BufferObject(BufferTarget.ArrayBuffer);
            buffers[index].SetData(data, BufferUsageHint.StaticDraw);
        }
    }
}
