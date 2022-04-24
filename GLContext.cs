using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class GLContext
    {
        //Todo maybe relocate. This is to quickly access the current context (which is only one atm)
        public static GLContext ActiveContext = null;

        /// <summary>
        /// Default rendering arguments when a rendered frame is being drawn.
        /// </summary>
        public RenderFrameArgs FrameArgs = new RenderFrameArgs();

        /// <summary>
        /// The screen buffer storing the current color/depth render texture of the drawn scene objects.
        /// </summary>
        public Framebuffer ScreenBuffer { get; set; }

        /// <summary>
        /// Selection tools used for selecting scene objects in different ways.
        /// </summary>
        public SelectionToolEngine SelectionTools = new SelectionToolEngine();

        /// <summary>
        /// Transform tools for transforming scene objects.
        /// </summary>
        public TransformEngine TransformTools = new TransformEngine();

        /// <summary>
        /// Picking toolsets for picking objects.
        /// </summary>
        public PickingTool PickingTools = new PickingTool();

        /// <summary>
        /// A toolset for linking IObjectLink objects to a transformable object.
        /// </summary>
        public LinkingTool LinkingTools = new LinkingTool();

        /// <summary>
        /// 
        /// </summary>
        public BoxCreationTool BoxCreationTool = new BoxCreationTool();

        /// <summary>
        /// Color picking for picking scene objects using a color ID pass.
        /// </summary>
        public ColorPicker ColorPicker = new ColorPicker();

        /// <summary>
        /// Ray picking for picking scene objects using bounding radius/boxes.
        /// </summary>
        public RayPicking RayPicker = new RayPicking();

        /// <summary>
        /// Collision ray picking for dropping scene objects to collision.
        /// </summary>
        public CollisionRayCaster CollisionCaster = new CollisionRayCaster();

        /// <summary>
        /// The camera instance to use in the scene.
        /// </summary>
        public Camera Camera { get; set; }

        /// <summary>
        /// The scene information containing the list of drawables along with selection handling.
        /// </summary>
        public GLScene Scene = new GLScene();

        /// <summary>
        /// The width of the current context. Should be given the viewport width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the current context. Should be given the viewport height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// If the current context is in focus or not.
        /// </summary>
        public bool Focused { get; set; } = true;

        /// <summary>
        /// Gets or sets the mouse position after a mouse down event.
        /// </summary>
        public Vector2 MouseOrigin { get; set; }

        /// <summary>
        /// Gets or sets the current mouse postion.
        /// </summary>
        public Vector2 CurrentMousePoint = Vector2.Zero;

        /// <summary>
        /// Gets or sets the offset from the mouse origin.
        /// </summary>
        public Vector2 MouseOffset => CurrentMousePoint - MouseOrigin;

        /// <summary>
        /// Determines to enable SRGB or not for the current context.
        /// </summary>
        public bool UseSRBFrameBuffer;

        /// <summary>
        /// Toggles bloom usage.
        /// </summary>
        public bool EnableBloom;

        /// <summary>
        /// Toggles fog usage.
        /// </summary>
        public bool EnableFog = true;

        /// <summary>
        /// Toggles dropping objects to collision of the current CollisionCaster or not
        /// during a translation transform.
        /// </summary>
        public bool EnableDropToCollision = true;

        /// <summary>
        /// The preview scale to scale up model displaying.
        /// </summary>
        public static float PreviewScale
        {
            get { return ActiveContext._previewScale; }
            set { ActiveContext._previewScale = value; }
        }

        private float _previewScale = 1.0f;

        /// <summary>
        /// Updates the viewport to draw a frame.
        /// </summary>
        public bool UpdateViewport { get; set; }

        /// <summary>
        /// Disables camera movement in the context.
        /// </summary>
        public bool DisableCameraMovement = false;

        /// <summary>
        /// Drawer to draw 2D elements onto the screen.
        /// </summary>
        public UIDrawer UIDrawer = new UIDrawer();

        public GLContext() { Init(); }

        /// <summary>
        /// Inits the resources used by the context.
        /// </summary>
        private void Init() {
            TransformTools.Init();
        }

        /// <summary>
        /// Sets the active context used in the viewer.
        /// </summary>
        public void SetActive() {
            GLContext.ActiveContext = this;
        }

        /// <summary>
        /// Enables the second color pass alpha to be used as a selection mask
        /// </summary>
        public void EnableSelectionMask() {
            GL.ColorMask(1, true, true, true, true);
        }

        /// <summary>
        /// Disables the second color pass alpha to be used as a selection mask
        /// </summary>
        public void DisableSelectionMask() {    
            GL.ColorMask(1, false, false, false, false);
        }

        /// <summary>
        /// Gets or sets the 3D Cursor position used for placing objects and custom origin points.
        /// </summary>
        public Vector3 Cursor3DPosition
        {
            get
            {
                if (Scene.Cursor3D == null)
                    return Vector3.Zero;

              return Scene.Cursor3D.Transform.Position;
            }
            set
            {
                if (Scene.Cursor3D != null) {
                    Scene.Cursor3D.Transform.Position = value;
                    Scene.Cursor3D.Transform.UpdateMatrix(true);
                }
            }
        }

        /// <summary>
        /// Gets the camera ray of the current mouse point on screen/
        /// <returns></returns>
        public CameraRay PointScreenRay() => CameraRay.PointScreenRay((int)CurrentMousePoint.X, (int)CurrentMousePoint.Y, Camera);

        /// <summary>
        /// Gets the camera ray of the given x y screen scoordinates.
        /// <returns></returns>
        public CameraRay PointScreenRay(int x, int y) => CameraRay.PointScreenRay(x, y, Camera);

        /// <summary>
        /// Gets the x y screen coordinates from a 3D position.
        /// </summary>
        public Vector2 WorldToScreen(Vector3 coord)
        {
            Vector3 vec = Vector3.Project(coord, 0, 0, Width, Height, -1, 1, Camera.ViewMatrix * Camera.ProjectionMatrix);
            return new Vector2((int)vec.X, Height - (int)(vec.Y));
        }

        /// <summary>
        /// Gets a 3D position of the current mouse coordinates given the depth.
        /// </summary>
        public Vector3 GetPointUnderMouse(float depth, bool useMouseDepth = false) {
            return ScreenToWorld(this.CurrentMousePoint.X, this.CurrentMousePoint.Y, depth, useMouseDepth);
        }

        /// <summary>
        /// Gets a 3D position given the screen x y coordinates.
        /// The depth is fixed to 100 unless the mouse depth is used in given arguments.
        /// </summary>
        public Vector3 ScreenToWorld(float x, float y, bool useMouseDepth = false){
            return ScreenToWorld(x, y, 100, useMouseDepth);
        }

        /// <summary>
        /// Gets a 3D position given the screen x y coordinates and depth.
        /// </summary>
        public Vector3 ScreenToWorld(float x, float y, float depth, bool useMouseDepth = false)
        {
            //The depth pixel from mouse cursor
            //This will place the selected point near rendered models.
            if (useMouseDepth && ColorPicker.NormalizedPickingDepth != Camera.ZFar)
                depth = ColorPicker.NormalizedPickingDepth;

            var ray = PointScreenRay((int)x, (int)y);
            return ray.Origin.Xyz + (ray.Direction * depth);
        }

        /// <summary>
        /// Converts the given mouse coordinates to normalized coordinate space to use in the GL coordinate system -1 1.
        /// </summary>
        public Vector2 NormalizeMouseCoords(Vector2 mousePos)
        {
            return new Vector2(
                 2.0f * mousePos.X / Width - 1,
                 -(2.0f * mousePos.Y / Height - 1));
        }

        /// <summary>
        /// Checks if the given shader is active in the context.
        /// </summary>
        public bool IsShaderActive(ShaderProgram shader) {
            return shader != null && shader.program == CurrentShader.program;
        }

        private ShaderProgram shader;

        /// <summary>
        /// The current shader in the context to be drawn.
        /// If null, will disable drawing the shader.
        /// </summary>
        public ShaderProgram CurrentShader
        {
            get { return shader; }
            set
            {
                //Disable active shader when given a null value
                if(value == null) {
                    GL.UseProgram(0);
                    return;
                }

                shader = value;
                //Force enable the shader. 
                //Could check to only do this only when the shader is changed but some shaders may still use higher level GL.UseProgram() atm
                shader.Enable();

                //Update standard camera matrices
                var mtxMdl = Camera.ModelMatrix;
                var mtxCam = Camera.ViewProjectionMatrix;
                shader.SetMatrix4x4("mtxMdl", ref mtxMdl);
                shader.SetMatrix4x4("mtxCam", ref mtxCam);
            }
        }

        private Vector2 _previousPosition;

        public void OnMouseMove(MouseEventInfo e, KeyEventInfo k, bool mouseDown)
        {
            UpdateViewport = true;

            //Set a saved mouse point to use in the application
            CurrentMousePoint = new Vector2(e.X, e.Y);

            SelectionTools.OnMouseMove(this, e);
            Scene.OnMouseMove(this, e);
            LinkingTools.OnMouseMove(this, e);
            BoxCreationTool.OnMouseMove(this, e);

            int transformState = 0;
            //Transforming can occur on shortcut keys rather than just mouse down
            if (TransformTools.ActiveActions.Count > 0)
                transformState = TransformTools.OnMouseMove(this, e);

            if (mouseDown)
            {
                if (!e.HasValue)
                    e.Position = new System.Drawing.Point((int)_previousPosition.X, (int)_previousPosition.Y);

                if (transformState != 0 || SelectionTools.IsActive || LinkingTools.IsActive || DisableCameraMovement)
                    return;

                if (e.RightButton == OpenTK.Input.ButtonState.Pressed)
                    MouseEventInfo.CursorHiddenMode = true;

                Camera.Controller.MouseMove(e, k, _previousPosition);
                _previousPosition = new OpenTK.Vector2(e.X, e.Y);
            }

            PickingTools.OnMouseMove(this, e);
        }

        private float previousMouseWheel;

        public void ResetPrevious() {
            previousMouseWheel = 0;
        }

        public void OnMouseWheel(MouseEventInfo e, KeyEventInfo k)
        {
            UpdateViewport = true;

            if (previousMouseWheel == 0)
                previousMouseWheel = e.WheelPrecise;

            e.Delta = e.WheelPrecise - previousMouseWheel;

            if (SelectionTools.IsActive) {
                SelectionTools.OnMouseWheel(this, e);
            }
            else
            {
                Camera.Controller.MouseWheel(e, k);
            }
            previousMouseWheel = e.WheelPrecise;
        }

        public void OnMouseUp(MouseEventInfo e)
        {
            UpdateViewport = true;

            MouseEventInfo.CursorHiddenMode = false;
            TransformTools.OnMouseUp(this, e);
            _previousPosition = new OpenTK.Vector2(e.X, e.Y);

            SelectionTools.OnMouseUp(this, e);
            BoxCreationTool.OnMouseUp(this, e);
            Scene.OnMouseUp(this, e);
            PickingTools.OnMouseUp(this, e);
            LinkingTools.OnMouseUp(this, e);
        }

        public void OnKeyDown(KeyEventInfo keyInfo, bool isRepeat, bool viewportFoucsed)
        {
            if (viewportFoucsed)
            {
                SelectionTools.OnKeyDown(this, keyInfo);
                TransformTools.OnKeyDown(this, keyInfo);

                if (!isRepeat)
                    Camera.KeyPress(keyInfo);
            }
            Scene.OnKeyDown(this, keyInfo, isRepeat);
        }

        public void OnMouseDown(MouseEventInfo e, KeyEventInfo k)
        {
            UpdateViewport = true;

            MouseOrigin = new Vector2(e.X, e.Y);
            Scene.OnMouseDown(this, e);

            _previousPosition = new OpenTK.Vector2(e.X, e.Y);

            SelectionTools.OnMouseDown(this, e);
            LinkingTools.OnMouseDown(this, e);
            BoxCreationTool.OnMouseDown(this, e);

            if (!TransformTools.Enabled || SelectionTools.IsActive || 
                LinkingTools.IsActive || BoxCreationTool.IsActive) //Skip picking, transforming and camera events for selection tools
                return;

            if (TransformTools.ActiveActions.Count > 0 && Scene.GetSelected().Count > 0)
            {
                var state = TransformTools.OnMouseDown(this, e);
                if (state != 0) //Skip picking and camera events for transforming objects
                    return;
            }
            //Transform is in a moving state through shortcut keys
            //Don't apply deselection from picking during the mouse down when it applies
            if (TransformTools.ReleaseTransform) {
                TransformTools.ReleaseTransform = false;
                return;
            }

            PickingTools.OnMouseDown(this, e);
            Camera.Controller.MouseClick(e, k);
        }

        public int ViewportX = 0;
        public int ViewportY = 0;
        public int ViewportWidth;
        public int ViewportHeight;

        public void SetViewportSize()
        {
            ViewportWidth = this.Width;
            ViewportHeight = this.Height;
            ViewportX = 0;
            ViewportY = 0;
            GL.Viewport(ViewportX, ViewportY, ViewportWidth, ViewportHeight);
        }
    }
}
