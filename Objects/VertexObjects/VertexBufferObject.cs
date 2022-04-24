using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace GLFrameworkEngine
{
    /// <summary>
    /// A vertex buffer object that supports loading multiple attributes and buffers.
    /// </summary>
    public struct VertexBufferObject : IDisposable
    {
        private int ID;
        private readonly int? indexBuffer;
        private int? instancedBuffer;
        private readonly Dictionary<object, VertexAttribute> attributes;
        private readonly Dictionary<object, VertexAttribute> attributesInstanced;

        private readonly List<int> bufferList;

        private bool _disposed;

        public VertexBufferObject(int[] buffer, int? indexBuffer = null, int? instancedBuffer = null)
        {
            ID = -1;
            this.indexBuffer = indexBuffer;
            this.instancedBuffer = instancedBuffer;
            this._disposed = false;
            attributes = new Dictionary<object, VertexAttribute>();
            attributesInstanced = new Dictionary<object, VertexAttribute>();
            bufferList = new List<int>();
            bufferList.AddRange(buffer);
        }

        public VertexBufferObject(int buffer, int? indexBuffer = null, int? instancedBuffer = null)
        {
            ID = -1;
            this.indexBuffer = indexBuffer;
            this.instancedBuffer = instancedBuffer;
            this._disposed = false;
            attributes = new Dictionary<object, VertexAttribute>();
            attributesInstanced = new Dictionary<object, VertexAttribute>();
            bufferList = new List<int>();
            bufferList.Add(buffer);
        }

        public void UpdateInstanceBuffer(int instancedBuffer) {
            this.instancedBuffer = instancedBuffer;
        }

        public bool ContainsAttribute(string name) {
            return attributes.ContainsKey(name);
        }

        public bool ContainsAttribute(int location) {
            return attributes.ContainsKey(location);
        }

        public void Clear()
        {
            attributes.Clear();
            attributesInstanced.Clear();
        }

        public void AddAttribute(int location, int size, VertexAttribPointerType type, bool normalized, int stride, int offset, int bufferIndex = 0)
        {
            attributes.Add(location, new VertexAttribute(size, type, normalized, stride, offset, false, 1, bufferIndex));
        }

        public void AddAttribute(string name, int size, VertexAttribPointerType type, bool normalized, int stride, int offset, int bufferIndex = 0)
        {
            attributes.Add(name, new VertexAttribute(size, type, normalized, stride, offset, false, 1, bufferIndex));
        }

        public void AddInstancedAttribute(string name, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            attributesInstanced.Add(name, new VertexAttribute(size, type, normalized, stride, offset, true));
        }
        public void AddInstancedAttribute(int location, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            attributesInstanced.Add(location, new VertexAttribute(size, type, normalized, stride, offset, true));
        }

        public void Initialize()
        {
            if (_disposed || ID != -1)
                return;

            GL.GenVertexArrays(1, out int vao);
            ID = vao;
            Bind();

            if (GLErrorHandler.CheckGLError()) Debugger.Break();
        }

        public void Enable(ShaderProgram shader)
        {
            if (_disposed) return;

            GL.BindVertexArray(ID);
            EnableAttributes(shader, attributes, bufferList.ToArray());

            if (instancedBuffer != null) {
                EnableAttributes(shader, attributesInstanced, instancedBuffer.Value);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void EnableAttributes(ShaderProgram shader, Dictionary<object, VertexAttribute> attributes, params int[] bufferIDs)
        {
            foreach (KeyValuePair<object, VertexAttribute> a in attributes)
            {
                int location = -1;
                if (a.Key is string)
                    location = shader.GetAttribute((string)a.Key);
                else
                    location = (int)a.Key;

                if (location == -1)
                    continue;

                GL.EnableVertexAttribArray(location);
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIDs[a.Value.bufferIndex]);

                if (a.Value.type == VertexAttribPointerType.Int)
                    GL.VertexAttribIPointer(location, a.Value.size, VertexAttribIntegerType.Int, a.Value.stride, new System.IntPtr(a.Value.offset));
                else
                    GL.VertexAttribPointer(location, a.Value.size, a.Value.type, a.Value.normalized, a.Value.stride, a.Value.offset);

                if (a.Value.instance)
                    GL.VertexAttribDivisor(location, a.Value.divisor);
                else
                    GL.VertexAttribDivisor(location, 0);
            }
        }

        private void DisableAttributes(ShaderProgram shader, Dictionary<object, VertexAttribute> attributes, params int[] bufferIDs)
        {
            foreach (KeyValuePair<object, VertexAttribute> a in attributes)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferIDs[a.Value.bufferIndex]);

                int location = -1;
                if (a.Key is string)
                    location = shader.GetAttribute((string)a.Key);
                else
                    location = (int)a.Key;

                if (location == -1)
                    continue;

                GL.DisableVertexAttribArray(location);
            }
        }

        public void Disable(ShaderProgram shader)
        {
            if (_disposed) return;

            DisableAttributes(shader, attributes, bufferList.ToArray());
            if (instancedBuffer != null) {
                DisableAttributes(shader, attributesInstanced, instancedBuffer.Value);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void BindVertexArray()
        {
            GL.BindVertexArray(ID);
        }

        public void Bind()
        {
            if (_disposed) return;

            GL.BindVertexArray(ID);
        }

        public void Use()
        {
            if (_disposed) return;

            GL.BindVertexArray(ID);
            if (indexBuffer.HasValue)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.Value);
        }

        public void Dispose()
        {
            if (bufferList == null)
                return;

            GL.DeleteVertexArray(ID);
            foreach (var buffer in bufferList)
                GL.DeleteBuffer(buffer);
            if (indexBuffer.HasValue)
                GL.DeleteBuffer(indexBuffer.Value);
            if (instancedBuffer.HasValue)
                GL.DeleteBuffer(instancedBuffer.Value);

            _disposed = true;
            ID = -1;
            bufferList.Clear();
            attributes.Clear();
        }

        private struct VertexAttribute
        {
            public int size;
            public VertexAttribPointerType type;
            public bool normalized;
            public int stride;
            public int offset;
            public bool instance;
            public int divisor;
            public int bufferIndex;

            public VertexAttribute(int size, VertexAttribPointerType type, bool normalized, int stride, int offset, bool instance = false, int divisor = 1, int bufferIndex = 0)
            {
                this.size = size;
                this.type = type;
                this.normalized = normalized;
                this.stride = stride;
                this.offset = offset;
                this.instance = instance;
                this.divisor = divisor;
                this.bufferIndex = bufferIndex;
            }
        }
    }
}
