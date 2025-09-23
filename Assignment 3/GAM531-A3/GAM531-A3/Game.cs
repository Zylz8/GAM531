using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int vertexBufferHandle;
        private int elementBufferHandle;

        private float[] vertices =
        {
            // Position               
            -0.25f, -0.25f, -0.25f,   1f, 0f, 0f,  // back bottom left
             0.25f, -0.25f, -0.25f,   0f, 1f, 0f,  // back bottom right
             0.25f,  0.25f, -0.25f,   0f, 0f, 1f,  // back top right
            -0.25f,  0.25f, -0.25f,   1f, 1f, 0f,  // back top left
            -0.25f, -0.25f,  0.25f,   1f, 0f, 1f,  // front bottom left
             0.25f, -0.25f,  0.25f,   0f, 1f, 1f,  // front bottom right
             0.25f,  0.25f,  0.25f,   1f, 1f, 1f,  // front top right
            -0.25f,  0.25f,  0.25f,   0f, 0f, 0f   // front top left
        };

        // Indices 
        /*
          3 ----- 2           
          |       | This is the Back Face 
          |       |
          0 ----- 1

          7 ----- 6           
          |       | This is the Front Face 
          |       |
          4 ----- 5

        So to create the left Square we would need to use 4,7,0,3. We would need to create two triangles to make a square. triangle 1 = 4,7,0 triangle 2 = 3,0,7

        To create the right square we would need to use 1,2,5,6
        To create the top square (2 triangles) we would use 3,2,6,7
        To create the bottom square (2 triangles) we woulse use 0,1,4,5

         */
        private uint[] indices =
        {
            0, 1, 2, 2, 3, 0, // Back Square
            4, 5, 6, 6, 7, 4, // Front Square
            0, 4, 7, 7, 3, 0, // Left Side Square
            1, 5, 6, 6, 2, 1, // Right Side Sqaure
            3, 2, 6, 6, 7, 3, // Top Sqaure
            0, 1, 5, 5, 4, 0  // Bottom Square
        };

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.Size = new Vector2i(1280, 768);
            this.CenterWindow(this.Size);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(1.0f, 0.5f, 0f, 0.5f); // orange back ground
            GL.Enable(EnableCap.DepthTest);

            // VAO
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            // VBO
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // EBO
            elementBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);


            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            // vertex shader with model, view and projection matrices
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor;

                uniform mat4 uModel;
                uniform mat4 uView;
                uniform mat4 uProjection;

                out vec3 vColor;

                void main()
                {
                    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
                    vColor = aColor;
                }";

            string fragmentShaderCode = @"
                #version 330 core
                in vec3 vColor;
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(vColor, 1.0);
                }";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");


            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vertexArrayHandle);

            // get the uniform locations
            int modelLoc = GL.GetUniformLocation(shaderProgramHandle, "uModel");
            int viewLoc = GL.GetUniformLocation(shaderProgramHandle, "uView");
            int projLoc = GL.GetUniformLocation(shaderProgramHandle, "uProjection");

            float angle = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            Matrix4 model = Matrix4.CreateRotationX(angle) * Matrix4.CreateRotationY(angle * 0.5f); // rotate cube around X axis and rotates the cube aroud the Y axis at half the speed
            // Rotates around both the X and Y axis

            Matrix4 view = Matrix4.LookAt(new Vector3(1.5f, 1.5f, 2f), Vector3.Zero, Vector3.UnitY); // view matrix (camera looking a origin)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f); // projection matrix(perspective)

            // send model,view, and projection to shader
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0); // draw the triangles

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.DeleteBuffer(elementBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);
            
            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);
            base.OnUnload();
        }

        private void CheckShaderCompile(int shaderHandle, string shaderName)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
            }
        }

    }
}

