using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Character _character;                 // Handles animation state + UV selection
        private int _shaderProgram;                   // Linked GLSL program
        private int _vao, _vbo;                       // Geometry
        private int _texture;                         // Sprite sheet
        // Sprint
        private float _xPos = 400f;
        private float _speed = 100f;
        private float _sprintMultiplier = 2f;
        // Jump
        private float _yPos = 300f;
        private float _yVelocity = 0f;
        private const float Gravity = -500f;
        private bool _isJump = true;


        public SpriteAnimationGame()
            : base(
                new GameWindowSettings(),
                new NativeWindowSettings { Size = (800, 600), Title = "Sprite Animation" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 0f); // Transparent background (A=0)
            GL.Enable(EnableCap.Blend); // Enable alpha blending
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderProgram = CreateShaderProgram(); // Compile + link
            _texture = Texture.LoadTexture("Sprite_Character.png"); // Upload sprite sheet

            // Quad vertices: [pos.x, pos.y, uv.x, uv.y], centered model space
            float w = 32f, h = 64f;                                             // Half-size: results in 64x128 sprite
            float[] vertices =
            {
                -w, -h, 0f, 0f,
                 w, -h, 1f, 0f,
                 w,  h, 1f, 1f,
                -w,  h, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Attribute 0: vec2 position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // Attribute 1: vec2 texcoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.UseProgram(_shaderProgram);

            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "uTexture"), 0);

            Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, -1, 1);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref ortho);

            Matrix4 model = Matrix4.CreateTranslation(400, 300, 0);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);

            _character = new Character(_shaderProgram); // Initializes idle frame uniforms
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Read keyboard state -> map to Direction
            var keyboard = KeyboardState;
            Direction dir = Direction.None;

            bool sprinting = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            float moveSpeed = _speed * (sprinting ? _sprintMultiplier : 1f);

            if (keyboard.IsKeyDown(Keys.Right))
            {
                dir = Direction.Right;
                _xPos += moveSpeed * (float)e.Time;
            }
            else if (keyboard.IsKeyDown(Keys.Left))
            {
                dir = Direction.Left;
                _xPos -= moveSpeed * (float)e.Time;
            }

            _xPos = Math.Clamp(_xPos, 32f, 800f - 32f);

            if (keyboard.IsKeyDown(Keys.Up) && _isJump)
            {
                _yVelocity = 300f;
                _isJump = false;
            }

            _yVelocity += Gravity * (float)e.Time;
            _yPos += _yVelocity * (float)e.Time;

            if (_yPos <= 300f)
            {
                _yPos = 300f;
                _yVelocity = 0f;
                _isJump = true;
            }

            CharacterState newState;
            if (!_isJump)
                newState = CharacterState.Jumping;
            else if (dir != Direction.None)
                newState = CharacterState.Running;
            else
                newState = CharacterState.Idle;

            _character.SetState(newState, dir);
            _character.UpdateAnimation((float)e.Time);


            Matrix4 model = Matrix4.CreateTranslation(_xPos, _yPos, 0f);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind texture and VAO, then draw
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.BindVertexArray(_vao);

            _character.Render();

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            // Free GPU resources
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            base.OnUnload();
        }

        // --- Shader creation utilities ---------------------------------------------------------
        private int CreateShaderProgram()
        {
            // Vertex Shader: transforms positions, flips V in UVs (image origin vs GL origin)
            string vs = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 projection;
uniform mat4 model;
void main() {
    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
    vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y);
}";
            // Fragment Shader: samples sub-rect of the sheet using uOffset/uSize
            string fs = @"
#version 330 core
in vec2 vTexCoord;
out vec4 color;
uniform sampler2D uTexture;
uniform vec2 uOffset;
uniform vec2 uSize;
void main() {
    vec2 uv = uOffset + vTexCoord * uSize;
    color = texture(uTexture, uv);
}";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            CheckShaderCompile(v, "VERTEX");

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            CheckShaderCompile(f, "FRAGMENT");

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            CheckProgramLink(p);

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }

        private static void CheckShaderCompile(int shader, string stage)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0)
                throw new Exception($"{stage} SHADER COMPILE ERROR:\n{GL.GetShaderInfoLog(shader)}");
        }

        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0)
                throw new Exception($"PROGRAM LINK ERROR:\n{GL.GetProgramInfoLog(program)}");
        }
    }
}
