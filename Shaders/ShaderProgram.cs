using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Linq;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a shader program that stores GL shader data.
    /// </summary>
    public class ShaderProgram : IDisposable
    {
        /// <summary>
        /// The ID of the shader program.
        /// </summary>
        public int program;

        //The input attributes in the shader
        private Dictionary<string, int> attributes = new Dictionary<string, int>();
        //The uniforms in the shader
        private Dictionary<string, int> uniforms = new Dictionary<string, int>();
        private int activeAttributeCount;
        //The shader stages inn the program
        private HashSet<Shader> shaders = new HashSet<Shader>();
        //Feedback attribute varyings to store attribute data executed from shader code
        public string[] FeedbackVaryings = new string[0];
        //Check if the shader was linked
        public bool LinkSucessful = false;

        // This isn't in OpenTK's enums for some reason.
        // https://www.khronos.org/registry/OpenGL/api/GL/glcorearb.h
        private static readonly int GL_PROGRAM_BINARY_MAX_LENGTH = 0x8741;

        public ShaderProgram(Shader[] shaders)
        {
            program = GL.CreateProgram();
            LoadSource(shaders);
        }

        public ShaderProgram()
        {
            program = GL.CreateProgram();
        }

        public ShaderProgram(Shader vertexShader, Shader fragmentShader)
        {
            program = GL.CreateProgram();
            LoadSource(vertexShader, fragmentShader);
        }

        public ShaderProgram(byte[] binaryData, BinaryFormat format)
        {
            program = GL.CreateProgram();
            LoadBinary(binaryData, format);
        }


        /// <summary>
        /// Loads the shader from shader stages.
        /// </summary>
        public void LoadSource(Shader[] shaders)
        {
            foreach (Shader shader in shaders)
            {
                if (!this.shaders.Contains(shader))
                    this.shaders.Add(shader);
            }
            CompileShaders();
        }

        /// <summary>
        /// Loads the shader from vertex and fragment shader stages.
        /// </summary>
        public void LoadSource(Shader vertexShader, Shader fragmentShader)
        {
            if (!this.shaders.Contains(vertexShader))
                this.shaders.Add(vertexShader);
            if (!this.shaders.Contains(fragmentShader))
                this.shaders.Add(fragmentShader);

            CompileShaders();
        }

        /// <summary>
        /// Loads the shader from a dumped glsl binary.
        /// </summary>
        public void LoadBinary(byte[] binaryData, BinaryFormat format)
        {
            // Number of supported binary formats.
            int binaryFormatCount;
            GL.GetInteger(GetPName.NumProgramBinaryFormats, out binaryFormatCount);

            // Get all supported formats.
            int[] binaryFormats = new int[binaryFormatCount];
            GL.GetInteger(GetPName.ProgramBinaryFormats, binaryFormats);

            if (binaryFormats.Contains((int)format))
            {
                try
                {
                    GL.ProgramBinary(program, format, binaryData, binaryData.Length);
                }
                catch (AccessViolationException)
                {
                    // The binary is corrupt or the wrong format. 
                    LinkSucessful = false;
                    return;
                }

                LoadAttributes(program);
                LoadUniorms(program);

                LinkSucessful = true;
            }
        }

        /// <summary>
        /// Links the shader program.
        /// </summary>
        public void Link()
        {
            GL.LinkProgram(program);
        }

        /// <summary>
        /// Enables the shader program.
        /// </summary>
        public void Enable()
        {
            GL.UseProgram(program);
        }

        /// <summary>
        /// Disables the shader program.
        /// </summary>
        public void Disable()
        {
            GL.UseProgram(0);
        }

        /// <summary>
        /// Disposes the shader program.
        /// </summary>
        public void Dispose()
        {
            foreach (var shader in shaders)
                shader.Dispose();

            GL.DeleteProgram(program);
        }

        /// <summary>
        /// Loads a 2D texture into the shader given a uniform name, texture ID, and slot number.
        /// </summary>
        public void SetTexture2D(string uniform, int id, int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, id);
            this.SetInt(uniform, slot);
        }

        /// <summary>
        /// Loads a 2D texture into the shader given a texture instance, uniform name, and slot number.
        /// </summary>
        public void SetTexture(GLTexture tex, string uniform, int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            tex.Bind();
            this.SetInt(uniform, slot);
        }

        /// <summary>
        /// Loads a transform into the shader given a uniform name and gl transform.
        /// </summary>
        public void SetTransform(string name, GLTransform transform) {
            var matrix = transform.TransformMatrix;
            SetMatrix4x4(name, ref matrix);
        }

        public void SetVector4(string name, Vector4 value)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform4(uniforms[name], value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform3(uniforms[name], value);
        }

        public void SetVector2(string name, Vector2 value)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform2(uniforms[name], value);
        }

        public void SetFloat(string name, float value)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform1(uniforms[name], value);
        }

        public void SetInt(string name, int value)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform1(uniforms[name], value);
        }

        public void SetBool(string name, bool value)
        {
            int intValue = value == true ? 1 : 0;

            if (uniforms.ContainsKey(name))
                GL.Uniform1(uniforms[name], intValue);
        }

        public void SetBoolToInt(string name, bool value)
        {
            if (!uniforms.ContainsKey(name))
                return;

            if (value)
                GL.Uniform1(uniforms[name], 1);
            else
                GL.Uniform1(this[name], 0);
        }

        public void SetColor(string name, System.Drawing.Color color)
        {
            if (uniforms.ContainsKey(name))
                GL.Uniform4(uniforms[name], color.R, color.G, color.B, color.A);
        }

        public void SetMatrix4x4(string name, ref Matrix4 value, bool transpose = false)
        {
            if (uniforms.ContainsKey(name))
                GL.UniformMatrix4(uniforms[name], transpose, ref value);
        }

        public int this[string name]
        {
            get { return uniforms[name]; }
        }   

        private void LoadUniorms(int program)
        {
            uniforms.Clear();

            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out activeAttributeCount);
            for (int i = 0; i < activeAttributeCount; i++)
            {
                string name = GL.GetActiveUniform(program, i, out int size, out ActiveUniformType type);
                if (string.IsNullOrEmpty(name))
                    continue;

                int location = GL.GetUniformLocation(program, name);
                // Overwrite existing vertex attributes.
                uniforms[name] = location;
            }
        }

        private void LoadAttributes(int program)
        {
            attributes.Clear();

            GL.GetProgram(program, GetProgramParameterName.ActiveAttributes, out activeAttributeCount);
            for (int i = 0; i < activeAttributeCount; i++)
            {
                string name = GL.GetActiveAttrib(program, i, out int size, out ActiveAttribType type);
                int location = GL.GetAttribLocation(program, name);

                // Overwrite existing vertex attributes.
                attributes[name] = location;
            }
        }

        public int GetAttribute(string name)
        {
            if (string.IsNullOrEmpty(name) || !attributes.ContainsKey(name))
                return -1;
            else
                return attributes[name];
        }


        public void EnableVertexAttributes()
        {
            foreach (KeyValuePair<string, int> attrib in attributes)
                GL.EnableVertexAttribArray(attrib.Value);
        }

        public void DisableVertexAttributes()
        {
            foreach (KeyValuePair<string, int> attrib in attributes)
                GL.DisableVertexAttribArray(attrib.Value);
        }

        public void SaveBinary(string fileName)
        {
            CreateBinary(out byte[] binaryData, out BinaryFormat format);
            System.IO.File.WriteAllBytes(fileName + ".bin", binaryData);
            System.IO.File.WriteAllBytes(fileName + ".format", BitConverter.GetBytes((int)format));
        }

        private void CreateBinary(out byte[] binaryData, out BinaryFormat format)
        {
            GL.GetProgram(program, (GetProgramParameterName)GL_PROGRAM_BINARY_MAX_LENGTH, out int size);
            binaryData = new byte[size];
            GL.GetProgramBinary(program, size, out _, out format, binaryData);
        }

        public virtual void OnCompiled() { }

        private void CompileShaders()
        {
            foreach (Shader shader in shaders)
            {
                GL.AttachShader(program, shader.id);
            }

            //Before linking, attach feedback varyings
            TransformFeedbackVaryings(program);

            GL.LinkProgram(program);
            foreach (var shader in shaders)
            {
                Console.WriteLine($"{shader.type.ToString("g")}:");

                string log = GL.GetShaderInfoLog(shader.id);
                Console.WriteLine(log);
            }
            LoadAttributes(program);
            LoadUniorms(program);

            LinkSucessful = true;
        }

        private void TransformFeedbackVaryings(int program)
        {
            if (FeedbackVaryings.Length == 0)
                return;

            string[] varyings = this.FeedbackVaryings;
            GL.TransformFeedbackVaryings(program, varyings.Length, varyings, TransformFeedbackMode.SeparateAttribs);
        }
    }

    public class Shader : IDisposable
    {
        public Shader(string src, ShaderType type)
        {
            id = GL.CreateShader(type);
            GL.ShaderSource(id, src);
            GL.CompileShader(id);
            this.type = type;
        }

        public Shader(string src, ShaderType type, Dictionary<string, string> macros)
        {
            id = GL.CreateShader(type);
            GL.ShaderSource(id, CompileMacros(macros, src));
            GL.CompileShader(id);
            this.type = type;
        }

        private string CompileMacros(Dictionary<string, string> macros, string src)
        {
            var sb = new System.Text.StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
            {
                foreach (var line in src.Split("\n")) {
                    string value = line;
                    if (line.StartsWith("#define")) {
                        var macroName = line.Split()[1];
                        if (macros.ContainsKey(macroName))
                            value = string.Format("#define {0} {1}", macroName, macros[macroName]);
                    }
                    writer.WriteLine(value);
                }
            }
            return sb.ToString();
        }

        public string GetShaderSource()
        {
            string source = "";

            GL.GetShader(id, ShaderParameter.ShaderSourceLength, out int length);
            if (length != 0)
                GL.GetShaderSource(id, length, out _, out source);
            return source;
        }

        public string GetInfoLog()
        {
            return GL.GetShaderInfoLog(id);
        }

        public void Dispose()
        {
            GL.DeleteShader(id);
        }

        public ShaderType type;

        public int id;
    }

    public class FragmentShader : Shader
    {
        public FragmentShader(string src)
            : base(src, ShaderType.FragmentShader)
        {

        }
    }

    public class VertexShader : Shader
    {
        public VertexShader(string src)
            : base(src, ShaderType.VertexShader)
        {

        }
    }

    public class GeomertyShader : Shader
    {
        public GeomertyShader(string src)
            : base(src, ShaderType.GeometryShader)
        {

        }
    }
}
