using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class BufferObject : GLObject
    {
        /// <summary>
        /// 
        /// </summary>
        public BufferTarget Target { get; }

        /// <summary>
        /// 
        /// </summary>
        public int DataCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int DataStride { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int DataSizeInBytes => DataStride * DataCount;

        public BufferObject(BufferTarget target) : base(GL.GenBuffer())
        {
            Target = target;
        }

        public void Bind()
        {
            GL.BindBuffer(Target, ID);
        }

        public void SetData<T>(T[] data, BufferUsageHint hint) where T : struct
        {
            DataCount = data.Length;
            DataStride = Marshal.SizeOf(typeof(T));

            Bind();
            GL.BufferData(Target, DataSizeInBytes, data, hint);
            GL.BindBuffer(Target, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ID);
        }
    }
}
