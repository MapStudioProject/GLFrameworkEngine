using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a global shader cache to store in tool shaders.
    /// The shader paths are initiated and shaders are only loaded when used.
    /// </summary>
    public class GlobalShaders
    {
        static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
        static Dictionary<string, string> ShaderPaths = new Dictionary<string, string>();

        static bool intDefault = false;

        //Paths for all the shaders relative to the shader folder
        static void InitPaths() 
        {
            intDefault = true;

            ShaderPaths.Add("DEBUG", Path.Combine("Generic","Debug"));
            ShaderPaths.Add("BILLBOARD", Path.Combine("Billboard","BillboardTexture"));
            ShaderPaths.Add("CUBEMAP_HDRENCODE", Path.Combine("Cubemap","HdrEncode"));
            ShaderPaths.Add("CUBEMAP_HDRDECODE", Path.Combine("Cubemap","HdrDecode"));
            ShaderPaths.Add("EQUIRECTANGULAR", Path.Combine("Cubemap","Equirectangular"));
            ShaderPaths.Add("CUBEMAP_IRRADIANCE", Path.Combine("Cubemap","Irradiance"));
            ShaderPaths.Add("CUBEMAP_PREFILTER", Path.Combine("Cubemap","Prefilter"));
            ShaderPaths.Add("GIZMO", Path.Combine("Editor","Gizmo"));
            ShaderPaths.Add("IMAGE_EDITOR", Path.Combine("Editor","ImageEditor"));
            ShaderPaths.Add("UV_WINDOW", Path.Combine("Editor","UVWindow"));
            ShaderPaths.Add("EFFECTS_DEFAULT", Path.Combine("Effects","Default"));
            ShaderPaths.Add("BASIC", Path.Combine("Generic","Basic"));
            ShaderPaths.Add("LINE", Path.Combine("Generic","Line"));
            ShaderPaths.Add("LINE_DASHED", Path.Combine("Generic","LineDashed"));
            ShaderPaths.Add("TEXTURE_ICON", Path.Combine("Interface","TextureIcon"));
            ShaderPaths.Add("LIGHTMAP", Path.Combine("Lighting","Lightmap"));
            ShaderPaths.Add("PROBE", Path.Combine("Lighting","ProbeCubemap"));
            ShaderPaths.Add("LIGHTPREPASS", Path.Combine("Lighting","LightPrepass"));
            ShaderPaths.Add("PROBE_DRAWER", Path.Combine("Lighting","ProbeDrawer"));
            ShaderPaths.Add("PROBE_VOXEL", Path.Combine("Lighting","ProbeVoxelDrawer"));
            ShaderPaths.Add("LPP_CAUSTICS", Path.Combine("Lighting","Caustics"));
            ShaderPaths.Add("NORMALS", Path.Combine("Normals","Normals"));
            ShaderPaths.Add("BLOOM_EXTRACT", Path.Combine("PostFx","Bloom","BloomExtract"));
            ShaderPaths.Add("BLUR", Path.Combine("PostFx","Blur","GaussianBlur"));
            ShaderPaths.Add("COLOR_CORRECTION", Path.Combine("PostFx","Correction","ColorCorrection"));
            ShaderPaths.Add("FINALHDR", Path.Combine("PostFx","FinalHDR"));
            ShaderPaths.Add("SCREEN", Path.Combine("Screen","Screen"));
            ShaderPaths.Add("SHADOW", Path.Combine("Shadows","Shadow"));
            ShaderPaths.Add("SHADOWPREPASS", Path.Combine("Shadows","ShadowPrepass"));
            ShaderPaths.Add("SHADOWQUAD", Path.Combine("Shadows","ShadowQuad"));
            ShaderPaths.Add("NORMALIZE_DEPTH", Path.Combine("Utility","NormalizeDepth"));
            ShaderPaths.Add("PICKING", Path.Combine("Utility","Picking"));
            ShaderPaths.Add("SELECTION", Path.Combine("Utility","Selection"));
            ShaderPaths.Add("GRID", Path.Combine("Viewer","Grid"));
            ShaderPaths.Add("GRID_INFINITE", Path.Combine("Viewer","GridInfinite"));
            ShaderPaths.Add("LINKING", Path.Combine("Linking","Connection"));
            ShaderPaths.Add("CUBEMAP_FILTER", Path.Combine("CubemapFilter"));
            ShaderPaths.Add("IRRADIANCE_CUBEMAP", Path.Combine("IrradianceCubemap"));
            ShaderPaths.Add("LUT_DISPLAY", Path.Combine("LUT","LutDisplay"));
            ShaderPaths.Add("WIREFRAME", Path.Combine("Wireframe","Wireframe"));
        }

        public static void AddShader(string key, string relativePath) {
            if (!ShaderPaths.ContainsKey(key))
                ShaderPaths.Add(key, relativePath);
        }

        /// <summary>
        /// Gets a shader given a key to store them in and a shader file path.
        /// </summary>
        public static ShaderProgram GetShader(string key, string path)
        {
            if (!Shaders.ContainsKey(key)) {
                Shaders.Add(key, LoadShader(path));
                Shaders[key].Link();
            }
            return Shaders[key];
        }

        /// <summary>
        /// Gets a shader given a key they are stored in.
        /// </summary>
        public static ShaderProgram GetShader(string key)
        {
            //Load the shader if not initiated yet.
            if (!Shaders.ContainsKey(key))
                InitShader(key);

            //Return the shader from the global list if present
            if (Shaders.ContainsKey(key))
                return Shaders[key];

            return null;
        }

        //Setup the shader for the first time
        static void InitShader(string key)
        {
            Console.WriteLine($"InitShader {key}");

            if (!intDefault)
                InitPaths();

            if (ShaderPaths.ContainsKey(key))
                Shaders.Add(key, LoadShader(ShaderPaths[key]));
        }

        //Load the shader from a path
        static ShaderProgram LoadShader(string name)
        {
            List<Shader> shaders = new List<Shader>();

            string shaderFolder = Path.Combine(Runtime.ExecutableDir,"Lib","Shaders");
            string frag = Path.Combine(shaderFolder,$"{name}.frag");
            string vert = Path.Combine(shaderFolder,$"{name}.vert");
            string geom = Path.Combine(shaderFolder,$"{name}.geom");
            if (File.Exists(vert)) shaders.Add(new VertexShader(File.ReadAllText(vert)));
            if (File.Exists(frag)) shaders.Add(new FragmentShader(File.ReadAllText(frag)));
            if (File.Exists(geom)) shaders.Add(new GeomertyShader(File.ReadAllText(geom)));

            if (shaders.Count == 0)
                throw new Exception($"Failed to find shaders at {name}");

            return new ShaderProgram(shaders.ToArray());
        }
    }
}
