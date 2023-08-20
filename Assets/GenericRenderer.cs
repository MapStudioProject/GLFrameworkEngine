using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    public class GenericRenderer : EditableObject, IFrustumCulling
    {
        public Dictionary<string, TextureView> Textures = new Dictionary<string, TextureView>();

        public class TextureView
        {
            public STGenericTexture Texture = null;

            public STChannelType RedChannel = STChannelType.Red;
            public STChannelType GreenChannel = STChannelType.Green;
            public STChannelType BlueChannel = STChannelType.Blue;
            public STChannelType AlphaChannel = STChannelType.Alpha;

            public IRenderableTexture RenderTexture
            {
                get { return Texture.RenderableTex; }
                set { Texture.RenderableTex = value; }
            }

            public bool IsSRGB { get; set; }

            public uint Width { get; set; }
            public uint Height { get; set; }

            public TexFormat Format { get; set; }

            public TextureView(STGenericTexture texture)
            {
                Texture = texture;
                Width = texture.Width;
                Height = texture.Height;
                IsSRGB = texture.IsSRGB;
                Format = texture.Platform.OutputFormat;
                RedChannel = texture.RedChannel;
                GreenChannel = texture.GreenChannel;
                BlueChannel = texture.BlueChannel;
                AlphaChannel = texture.AlphaChannel;

                var watch = System.Diagnostics.Stopwatch.StartNew();

                texture.LoadRenderableTexture();

                watch.Stop();
            }

            public TextureView(GLTexture texture)
            {
                Width = (uint)texture.Width;
                Height = (uint)texture.Height;
                Format = TexFormat.RGBA8_UNORM;
                RenderTexture = texture;            }
        }

        public List<ModelAsset> Models = new List<ModelAsset>();

        public EventHandler TransformChanged;
        public EventHandler TransformApplied;

        public bool EnableFrustumCulling => true;

        public string ID { get; set; }

        public virtual bool EnableShadows { get; set; }

        public virtual bool InFrustum { get; set; } = true;

        public Vector4 BoundingSphere { get; set; }

        public virtual GLFrameworkEngine.ShaderProgram GetShaderProgram() { return null; }

        public virtual List<string> DebugShading { get; }

        public virtual string DebugShadingMode { get; set; }

        //Note this is necessary to adjust if meshes are animated by shaders
        //For animated meshes use the normal vertex shader, then a picking color fragment shader
        //This is only for static meshes
        public virtual GLFrameworkEngine.ShaderProgram PickingShader => GLFrameworkEngine.GlobalShaders.GetShader("PICKING");

        public virtual void ResetAnimations() { }

        public virtual void ReloadFile(string fileName) { }

        bool UpdateModelMatrix = false;

        public string Name { get; set; }

        public void ResetAnim()
        {
        }

        public GenericRenderer() : base(null)
        {
            UpdateModelMatrix = true;
        }

        public GenericRenderer(NodeBase parent) : base(parent)
        {
            UpdateModelMatrix = true;
        }

        public virtual bool IsInsideFrustum(GLFrameworkEngine.GLContext control)
        {
            return true;
        }

        public virtual void OnModelMatrixUpdated()
        {

        }

        public virtual void DrawColorBufferPass(GLContext control)
        {

        }

        public virtual void DrawShadowModel(GLContext control)
        {

        }

        public virtual void DrawGBuffer(GLContext control)
        {

        }

        public virtual void DrawCaustics(GLContext control, GLTexture gbuffer, GLTexture linearDepth)
        {

        }

        public virtual void DrawCubeMapScene(GLContext control)
        {

        }

        public virtual void DrawSelection(GLContext control)
        {

        }

        public virtual void Dispose()
        {

        }
    }
}
