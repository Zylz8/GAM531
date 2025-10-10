using System;
using WindowEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowEngine
{
    public class Window : GameWindow
    {
        private readonly float[] _vertices =
        {
            // vertices for the cube
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f
        };

        private readonly uint[] _indices =
        {
            0, 1, 2, 2, 3, 0,   // back
            4, 5, 6, 6, 7, 4,   // front
            0, 1, 5, 5, 4, 0,   // bottom
            2, 3, 7, 7, 6, 2,   // top
            0, 3, 7, 7, 4, 0,   // left
            1, 2, 6, 6, 5, 1    // right
        };

        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private Shader _shader;
        private Texture _texture;
        private Texture _texture2;
        private double _time;


        // Cameras
        private Vector3 _cameraPosition = new Vector3(2f, 2f, 2f);
        private Vector3 _cameraFront = -Vector3.Normalize(new Vector3(2f, 2f, 2f)); 
        private Vector3 _cameraUp = Vector3.UnitY;

        // movement up/down   left/right
        private float _pitch = 0f; // 
        private float _yaw = -90f; // 
        private float _sensitivity = 0.1f;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            int vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // This make the mouse cursor invisible and captured
            CursorState = CursorState.Grabbed;

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;
            float cameraSpeed = 2.0f * (float)e.Time; 

            if (input.IsKeyDown(Keys.Escape))
                Close();

            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(_cameraUp, _cameraFront));


            if (input.IsKeyDown(Keys.W))
            {
                _cameraPosition += _cameraFront * cameraSpeed; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _cameraPosition -= _cameraFront * cameraSpeed; // Backward
            }
                if (input.IsKeyDown(Keys.A))
            {
                _cameraPosition += cameraRight * cameraSpeed;  // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _cameraPosition -= cameraRight * cameraSpeed;  // Right
            }

            // Mouse
            var mouse = MouseState;
            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                float deltaX = mouse.X - _lastPos.X;
                float deltaY = _lastPos.Y - mouse.Y; 

                _lastPos = new Vector2(mouse.X, mouse.Y);

                _yaw += deltaX * _sensitivity;
                _pitch += deltaY * _sensitivity;

                // Clamp pitch
                _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

                // Update front vector
                Vector3 front;
                front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
                front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
                front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
                _cameraFront = Vector3.Normalize(front);
            }
        }



        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _time += e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();

            var model = Matrix4.CreateRotationY((float)_time);

            // Change the view matrix.LookAt to look at the camera position
            var view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);

            var projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                Size.X / (float)Size.Y,
                0.1f,
                100f);

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }
    }
}
