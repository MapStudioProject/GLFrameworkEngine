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

            ShaderPaths.Add("DEBUG", "Generic\\Debug");
            ShaderPaths.Add("BILLBOARD", "Billboard\\BillboardTexture");
            ShaderPaths.Add("CUBEMAP_HDRENCODE", "Cubemap\\HdrEncode");
            ShaderPaths.Add("CUBEMAP_HDRDECODE", "Cubemap\\HdrDecode");
            ShaderPaths.Add("EQUIRECTANGULAR", "Cubemap\\Equirectangular");
            ShaderPaths.Add("CUBEMAP_IRRADIANCE", "Cubemap\\Irradiance");
            ShaderPaths.Add("CUBEMAP_PREFILTER", "Cubemap\\Prefilter");
            ShaderPaths.Add("GIZMO", "Editor\\Gizmo");
            ShaderPaths.Add("IMAGE_EDITOR", "Editor\\ImageEditor");
            ShaderPaths.Add("UV_WINDOW", "Editor\\UVWindow");
            ShaderPaths.Add("EFFECTS_DEFAULT", "Effects\\Default");
            ShaderPaths.Add("BASIC", "Generic\\Basic");
            ShaderPaths.Add("LINE", "Generic\\Line");
            ShaderPaths.Add("LINE_DASHED", "Generic\\LineDashed");
            ShaderPaths.Add("TEXTURE_ICON", "Interface\\TextureIcon");
            ShaderPaths.Add("LIGHTMAP", "Lighting\\Lightmap");
            ShaderPaths.Add("PROBE", "Lighting\\ProbeCubemap");
            ShaderPaths.Add("LIGHTPREPASS", "Lighting\\LightPrepass");
            ShaderPaths.Add("PROBE_DRAWER", "Lighting\\ProbeDrawer");
            ShaderPaths.Add("PROBE_VOXEL", "Lighting\\ProbeVoxelDrawer");
            ShaderPaths.Add("LPP_CAUSTICS", "Lighting\\Caustics");
            ShaderPaths.Add("NORMALS", "Normals\\Normals");
            ShaderPaths.Add("BLOOM_EXTRACT", "PostFx\\Bloom\\BloomExtract");
            ShaderPaths.Add("BLUR", "PostFx\\Blur\\GaussianBlur");
            ShaderPaths.Add("COLOR_CORRECTION", "PostFx\\Correction\\ColorCorrection");
            ShaderPaths.Add("FINALHDR", "PostFx\\FinalHDR");
            ShaderPaths.Add("SCREEN", "Screen\\Screen");
            ShaderPaths.Add("SHADOW", "Shadows\\Shadow");
            ShaderPaths.Add("SHADOWPREPASS", "Shadows\\ShadowPrepass");
            ShaderPaths.Add("SHADOWQUAD", "Shadows\\ShadowQuad");
            ShaderPaths.Add("NORMALIZE_DEPTH", "Utility\\NormalizeDepth");
            ShaderPaths.Add("PICKING", "Utility\\Picking");
            ShaderPaths.Add("SELECTION", "Utility\\Selection");
            ShaderPaths.Add("GRID", "Viewer\\Grid");
            ShaderPaths.Add("GRID_INFINITE", "Viewer\\GridInfinite");
            ShaderPaths.Add("LINKING", "Linking\\Connection");
            ShaderPaths.Add("CUBEMAP_FILTER", "CubemapFilter");
            ShaderPaths.Add("IRRADIANCE_CUBEMAP", "IrradianceCubemap");
            ShaderPaths.Add("LUT_DISPLAY", "LUT\\LutDisplay");
            ShaderPaths.Add("WIREFRAME", "Wireframe\\Wireframe");
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

            string shaderFolder = $"{Runtime.ExecutableDir}\\Lib\\Shaders\\";
            string frag = $"{shaderFolder}{name}.frag";
            string vert = $"{shaderFolder}{name}.vert";
            string geom = $"{shaderFolder}{name}.geom";
            if (File.Exists(vert)) shaders.Add(new VertexShader(File.ReadAllText(vert)));
            if (File.Exists(frag)) shaders.Add(new FragmentShader(File.ReadAllText(frag)));
            if (File.Exists(geom)) shaders.Add(new GeomertyShader(File.ReadAllText(geom)));

            if (shaders.Count == 0)
                throw new Exception($"Failed to find shaders at {name}");

            return new ShaderProgram(shaders.ToArray());
        }
    }
}
