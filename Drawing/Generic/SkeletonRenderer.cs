using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    public class SkeletonRenderer : RenderMesh<Vector4>, IDrawable, ITransformableObject, ISelectableContainer
    {
        /// <summary>
        /// The worldspace transform of the skeleton.
        /// </summary>
        public GLTransform Transform { get; set; } = new GLTransform();

        /// <summary>
        /// Determines if skeleton is hovered.
        /// </summary>
        public bool IsHovered { get; set; }

        /// <summary>
        /// Determines if skeleton is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Determines if skeleton can be selected.
        /// </summary>
        public bool CanSelect
        {
            get { return Runtime.DisplayBones && IsVisible && false; }
            set { }
        }

        /// <summary>
        /// The list of rendered bones.
        /// </summary>
        public List<BoneRender> Bones = new List<BoneRender>();

        /// <summary>
        /// The list of selectables for picking bones.
        /// </summary>
        public IEnumerable<ITransformableObject> Selectables => Bones;

        /// <summary>
        /// Determines if the skeleton is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get { return Skeleton.Visible; }
            set { Skeleton.Visible = value; }
        }

        /// <summary>
        /// Debug display for ray picking.
        /// </summary>
        static bool DEBUG_DISPLAY_RAYAABBB = false;

        //Bone color
        Vector4 BoneColor = new Vector4(0.95f, 0.95f, 0, 1.0f);
        Vector4 SelectedBoneColor = new Vector4(0.95f, 0.95f, 0.95f, 1.0f);

        private static Matrix4 prismRotation = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), 1.5708f);

        private STSkeleton Skeleton;

        public SkeletonRenderer(STSkeleton skeleton) : base(screenPositions, PrimitiveType.Lines)
        {
            Reload(skeleton);
        }

        public void Update()
        {
            Skeleton.Update();
        }

        public void Reload(STSkeleton skeleton)
        {
            Skeleton = skeleton;

            Bones.Clear();
            foreach (var bone in skeleton.Bones)
                Bones.Add(new BoneRender(bone));

            //Setup parenting
            foreach (var bone in Bones)
            {
                var parent = Bones.FirstOrDefault(x => x.BoneData == bone.BoneData.Parent);
                bone.SetParent(parent);
            }
        }

        ShaderProgram ShaderProgram;

        static string VertexShader = @"
            #version 330

            in vec4 vPosition;

            uniform mat4 mtxCam;
            uniform mat4 mtxMdl;

            uniform mat4 bone;
            uniform mat4 parent;
            uniform mat4 rotation;
            uniform mat4 ModelMatrix;
            uniform int hasParent;
            uniform float scale;

            void main()
            {
                vec4 position = bone * rotation * vec4(vPosition.xyz * scale, 1);
                if (hasParent == 1)
                {
                    if (vPosition.w == 0)
                        position = parent * rotation * vec4(vPosition.xyz * scale, 1);
                    else
                        position = bone * rotation * vec4((vPosition.xyz - vec3(0, 1, 0)) * scale, 1);
                }
	            gl_Position =  mtxCam  * mtxMdl * ModelMatrix * vec4(position.xyz, 1);
            }";

        static string FragShader = @"
            #version 330
            uniform vec4 boneColor;

            out vec4 FragColor;
            out vec4 brightnessOutput;

            void main(){
	            FragColor = boneColor;
                brightnessOutput = vec4(0);
            }";

        public void Prepare(GLContext control)
        {
            if (ShaderProgram != null)
                return;

            ShaderProgram = new ShaderProgram(
                new VertexShader(VertexShader),
                new FragmentShader(FragShader));
        }

        public void DrawModel(GLContext control, Pass pass)
        {
            if (Skeleton == null || pass == Pass.TRANSPARENT || !Runtime.DisplayBones)
                return;

            if (ShaderProgram == null)
                Prepare(control);

            control.CurrentShader = ShaderProgram;
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            ShaderProgram.SetMatrix4x4("rotation", ref prismRotation);
            //Draw connections
            foreach (var boneRender in this.Bones)
            {
                var bn = boneRender.BoneData;
                if (!bn.Visible)
                    continue;

                Matrix4 modelMatrix = Matrix4.Identity;

                ShaderProgram.SetVector4("boneColor", BoneColor);
                ShaderProgram.SetFloat("scale", Runtime.BonePointSize * Skeleton.PreviewScale);
                ShaderProgram.SetMatrix4x4("ModelMatrix", ref modelMatrix);

                Matrix4 transform = bn.Transform;

                ShaderProgram.SetMatrix4x4("bone", ref transform);
                ShaderProgram.SetInt("hasParent", bn.ParentIndex != -1 ? 1 : 0);

                if (bn.ParentIndex != -1)
                {
                    var transformParent = bn.Parent.Transform;
                    ShaderProgram.SetMatrix4x4("parent", ref transformParent);
                }

                Draw(ShaderProgram);
            }
            //Draw points by themselves
            foreach (var boneRender in this.Bones)
            {
                var bn = boneRender.BoneData;
                if (!bn.Visible)
                    continue;

                Matrix4 modelMatrix = Matrix4.Identity;

                ShaderProgram.SetVector4("boneColor", BoneColor);
                ShaderProgram.SetFloat("scale", Runtime.BonePointSize * Skeleton.PreviewScale);
                ShaderProgram.SetMatrix4x4("ModelMatrix", ref modelMatrix);

                Matrix4 transform = bn.Transform;

                ShaderProgram.SetMatrix4x4("bone", ref transform);

                if (boneRender.IsSelected)
                    ShaderProgram.SetVector4("boneColor", SelectedBoneColor);

                ShaderProgram.SetInt("hasParent", 0);
                Draw(ShaderProgram);
            }

            if (DEBUG_DISPLAY_RAYAABBB)
            {
                foreach (var boneRender in this.Bones)
                {
                    var mat = new StandardMaterial();
                    mat.ModelMatrix = boneRender.Transform.TransformMatrix;
                    mat.Render(control);
                    BoundingBoxRender.Draw(control, boneRender.GetRayBounding().Box);
                }
            }

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
        }

        private static Vector4[] screenPositions = new Vector4[]
        {
            // cube
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 0f, -1f, 0),

            //point top parentless
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 0),

            //point top
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 1),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, 1f, 0f, 1),

            //point bottom
            new Vector4(0f, 0f, -1f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(0f, 0f, 1f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(1f, 0f, 0f, 0),
            new Vector4(0f, -1f, 0f, 0),
            new Vector4(-1f, 0f, 0f, 0),
            new Vector4(0f, -1f, 0f, 0),
        };

        public class BoneRender : ITransformableObject, IRenderNode, IRayCastPicking
        {
            public BoneRender Parent { get; set; }

            public GLTransform Transform { get; set; } = new GLTransform();
            public bool IsHovered { get; set; }

            public bool IsSelected
            {
                get { return UINode.IsSelected; }
                set { UINode.IsSelected = value; }
            }

            public bool CanSelect
            {
                get { return Runtime.DisplayBones && BoneData.Visible && false; }
                set { }
            }

            public NodeBase UINode { get; set; }

            public STBone BoneData;

            public BoundingNode GetRayBounding()
            {
                return new BoundingNode()
                {
                    Radius = 0.35f,
                    Box = new BoundingBox(new Vector3(-0.7f), new Vector3(0.7f)),
                };
            }

            bool updating = false;
            bool boneUpdating = false;

            public BoneRender(STBone bone) {
                BoneData = bone;
                UINode = new NodeBase(bone.Name);
                UINode.Icon = '\uf5d7'.ToString();
                UINode.HasCheckBox = true;
                UINode.Tag = bone;
                UINode.OnChecked += delegate
                {
                    BoneData.Visible = UINode.IsChecked;
                };
                UINode.OnSelected += delegate
                {
                    GLContext.ActiveContext.Scene.OnSelectionChanged(GLContext.ActiveContext);

                };
                BoneData.TransformUpdated += delegate
                {
                    if (updating)
                        return;

                    boneUpdating = true;
                    //World position
                    var worldTransform = BoneData.Transform;
                    Transform.Position = worldTransform.ExtractTranslation();
                    Transform.Rotation = worldTransform.ExtractRotation();
                    Transform.Scale = worldTransform.ExtractScale();
                    Transform.UpdateMatrix(true);

                    boneUpdating = false;
                };

                //World position
                var worldTransform = BoneData.Transform;
                Transform.Position = worldTransform.ExtractTranslation();
                Transform.Rotation = worldTransform.ExtractRotation();
                Transform.Scale = worldTransform.ExtractScale();
                Transform.UpdateMatrix(true);

                Transform.TransformUpdated += delegate
                {
                    if (boneUpdating)
                        return;

                    updating = true;

                    //Turn into local space
                    var local = this.Transform.TransformMatrix;
                    if (BoneData.Parent != null)
                        local = Transform.TransformMatrix * BoneData.Parent.Transform.Inverted();

                    Vector3 position = local.ExtractTranslation();
                    var rotation = local.ExtractRotation();
                    var scale = local.ExtractScale();

                    BoneData.AnimationController.Position = position;
                    BoneData.AnimationController.Rotation = rotation;
                    BoneData.AnimationController.Scale = scale;
                    BoneData.Skeleton.Update();

                    updating = false;
                };
            }

            public void SetParent(BoneRender parent)
            {
                if (parent == null)
                    return;

                Parent = parent;
                parent.UINode.AddChild(UINode);
            }
        }
    }
}
