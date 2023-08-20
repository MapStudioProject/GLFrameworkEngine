using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public class GLSampler : GLObject
    {
        public TextureMagFilter MagFilter { get; private set; }
        public TextureMinFilter MinFilter { get; private set; }

        public TextureWrapMode WrapS { get; private set; }
        public TextureWrapMode WrapT { get; private set; }
        public TextureWrapMode WrapR { get; private set; }

        public GLSampler() : base(GL.GenSampler())
        {

        }

        public void SetMipmap(float min, float max, float bias)
        {
            SetSamplerParameter(SamplerParameterName.TextureMinLod, min);
            SetSamplerParameter(SamplerParameterName.TextureMaxLod, max);
            SetSamplerParameter(SamplerParameterName.TextureLodBias, bias);
        }

        public void SetFilter(TextureMinFilter min, TextureMagFilter mag)
        {
            MinFilter = min;
            MagFilter = mag;
            SetSamplerParameter(SamplerParameterName.TextureMinFilter, (int)min);
            SetSamplerParameter(SamplerParameterName.TextureMagFilter, (int)mag);
        }

        public void SetWrap(TextureWrapMode wrapS, TextureWrapMode wrapT, TextureWrapMode wrapR)
        {
            WrapS = wrapS;
            WrapT = wrapT;
            WrapR = wrapR;
            SetSamplerParameter(SamplerParameterName.TextureWrapS, (int)WrapS);
            SetSamplerParameter(SamplerParameterName.TextureWrapT, (int)WrapT);
            SetSamplerParameter(SamplerParameterName.TextureWrapR, (int)WrapR);
        }

        public void Bind(int textureUnit)
        {
            GL.BindSampler(textureUnit, ID);
        }


        private void SetSamplerParameter(SamplerParameterName parameter, int value)
        {
            GL.SamplerParameter(ID, parameter, value);
        }

        private void SetSamplerParameter(SamplerParameterName parameter, float value)
        {
            GL.SamplerParameter(ID, parameter, value);
        }
    }
}
