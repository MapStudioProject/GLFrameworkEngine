using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GLFrameworkEngine
{
    public interface ITransformAction
    {
        TransformSettings Settings { get; set; }

        TransformEngine.Axis Axis { get; set; }

        int ResetTransform(GLContext context, TransformSettings settings);
        int TransformChanged(GLContext context, float x, float y, TransformSettings settings);
        int FinishTransform();

        void ApplyTransform(GLContext context, List<GLTransform> previousTransforms, List<GLTransform> adjustedTransforms);
    }
}
