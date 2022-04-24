using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class TransformUndo : IRevertable
    {
        TransformInfo[] Info;

        public TransformUndo(GLTransform transform)
        {
            Info = new TransformInfo[1] { new TransformInfo(transform) };
        }

        public TransformUndo(GLTransform[] transforms)
        {
            Info = new TransformInfo[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
                Info[i] = new TransformInfo(transforms[i]);
        }

        public TransformUndo(TransformInfo info) {
            Info = new TransformInfo[1] { info };
        }

        public TransformUndo(TransformInfo[] info) {
            Info = info;
        }

        public IRevertable Revert()
        {
            var translateUndo = new TransformUndo(new TransformInfo[Info.Length]);

            for (int i = 0; i < Info.Length; i++)
            {
                translateUndo.Info[i] = new TransformInfo(Info[i].Transform);

                Info[i].Transform.Position = Info[i].PrevPosition;
                Info[i].Transform.Scale = Info[i].PrevScale;
                Info[i].Transform.Rotation = Info[i].PrevRotation;
                if (Info[i].Transform.HasCustomOrigin)
                    Info[i].Transform.SetCustomOrigin(Info[i].PrevOrigin);
                Info[i].Transform.UpdateMatrix(true);
            }

            GLContext.ActiveContext.TransformTools.UpdateOrigin();
            GLContext.ActiveContext.TransformTools.UpdateBoundingBox();
            return translateUndo;
        }
    }

    public class TransformInfo
    {
        public GLTransform Transform;
        public Vector3 PrevPosition;
        public Vector3 PrevScale;
        public Quaternion PrevRotation;
        public Vector3 PrevOrigin;

        public TransformInfo(GLTransform transform)
        {
            Transform = transform;
            PrevRotation = transform.Rotation;
            PrevScale = transform.Scale;
            PrevPosition = transform.Position;
            PrevOrigin = transform.Origin;
        }
    }
}
