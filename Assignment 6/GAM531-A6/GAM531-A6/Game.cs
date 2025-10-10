using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int elementBufferHandle;
        private int textureHandle;

        private int modelLoc, viewLoc, projLoc;
        private int lightPosLoc, viewPosLoc, lightColorLoc, objectColorLoc;

        private float fov = 60f; // starting FOV

        // Light
        private Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f); // position of the point light
        private Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f); // color of the light
        private Vector3 objectColor = new Vector3(1.0f, 0.5f, 0.31f);

        // Camera
        private Vector3 cameraPos = new Vector3(2.0f, 2.0f, 3.0f);
        private Vector3 cameraFront = -Vector3.UnitZ;
        private Vector3 cameraUp = Vector3.UnitY;
        private float hori = -90f; // left and right control
        private float vert = 0f; // up and down control
        private Vector2 lastMousePos;
        private bool firstMouse = true;


        // vertices : positions, u,v , normals
        private float[] vertices = {
            // positions       // u , v     // normals
             // Front face
            -0.5f,-0.5f, 0.5f,  0f,0f,   0f,0f,1f,
             0.5f,-0.5f, 0.5f,  1f,0f,   0f,0f,1f,
             0.5f, 0.5f, 0.5f,  1f,1f,   0f,0f,1f,
            -0.5f, 0.5f, 0.5f,  0f,1f,   0f,0f,1f,   

            // Back face
            -0.5f,-0.5f,-0.5f,  1f,0f,   0f,0f,-1f,
             0.5f,-0.5f,-0.5f,  0f,0f,   0f,0f,-1f,
             0.5f, 0.5f,-0.5f,  0f,1f,   0f,0f,-1f,
            -0.5f, 0.5f,-0.5f,  1f,1f,   0f,0f,-1f, 

            // Left face
            -0.5f,-0.5f,-0.5f,  0f,0f,  -1f,0f,0f,
            -0.5f,-0.5f, 0.5f,  1f,0f,  -1f,0f,0f,
            -0.5f, 0.5f, 0.5f,  1f,1f,  -1f,0f,0f,
            -0.5f, 0.5f,-0.5f,  0f,1f,  -1f,0f,0f,  

            // Right face
             0.5f,-0.5f,-0.5f,  1f,0f,   1f,0f,0f,
             0.5f,-0.5f, 0.5f,  0f,0f,   1f,0f,0f,
             0.5f, 0.5f, 0.5f,  0f,1f,   1f,0f,0f,
             0.5f, 0.5f,-0.5f,  1f,1f,   1f,0f,0f,  

            // Top face
            -0.5f, 0.5f, 0.5f,  0f,0f,   0f,1f,0f,
             0.5f, 0.5f, 0.5f,  1f,0f,   0f,1f,0f,
             0.5f, 0.5f,-0.5f,  1f,1f,   0f,1f,0f,
            -0.5f, 0.5f,-0.5f,  0f,1f,   0f,1f,0f,   

            // Bottom face
            -0.5f,-0.5f, 0.5f,  0f,1f,   0f,-1f,0f,
             0.5f,-0.5f, 0.5f,  1f,1f,   0f,-1f,0f,
             0.5f,-0.5f,-0.5f,  1f,0f,   0f,-1f,0f,
            -0.5f,-0.5f,-0.5f,  0f,0f,   0f,-1f,0f
        };

        private int[] indices = {
            0, 1, 2, 2, 3, 0,       // Front
            4, 5, 6, 6, 7, 4,       // Back
            8, 9,10,10,11, 8,       // Left
           12,13,14,14,15,12,       // Right
           16,17,18,18,19,16,       // Top
           20,21,22,22,23,20        // Bottom
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

        private readonly string vertexShaderCode = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexCoord;
            layout(location = 2) in vec3 aNormal;

            out vec3 FragPos;
            out vec3 Normal;
            out vec2 TexCoord;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            void main()
            {
                FragPos = vec3(model * vec4(aPosition, 1.0));
                Normal = mat3(transpose(inverse(model))) * aNormal;
                TexCoord = aTexCoord;
                gl_Position = projection * view * vec4(FragPos, 1.0);
            }
        ";

        private readonly string fragmentShaderCode = @"
            #version 330 core
            out vec4 FragColor;

            in vec3 FragPos;
            in vec3 Normal;
            in vec2 TexCoord;

            uniform vec3 lightPos; // Position of the point light
            uniform vec3 viewPos; // Camera Position
            uniform vec3 lightColor; // Color of the Light
            uniform vec3 objectColor; // Color of the object
            uniform sampler2D ourTexture;

            void main()
            {
                // Ambient
                float ambientStrength = 0.03;
                vec3 ambient = ambientStrength * lightColor;

                // Diffuse
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(lightPos - FragPos);
                float diff = pow(max(dot(norm, lightDir), 0.0), 1.3);
                vec3 diffuse = diff * lightColor;

                // Specular
                float specularStrength = 0.4;
                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                vec3 specular = specularStrength * spec * lightColor;

                vec3 textureColor = texture(ourTexture, TexCoord).rgb;
                // Combine Results
                vec3 result = (ambient + diffuse + specular) * textureColor * objectColor;

                FragColor = vec4(result, 1.0);
            }
        ";

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(1.0f, 0.5f, 0f, 0.5f); // orange back ground
            GL.Enable(EnableCap.DepthTest); // Cube not flat

            // VBO
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // EBO
            elementBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // VAO
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);

            // position(3) + uv(2) + normal(3 = 8 vertexs
            int vertexNum = 8 * sizeof(float);

            // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexNum, 0);
            GL.EnableVertexAttribArray(0);

            // texture coord
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexNum, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Normal
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexNum, 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);


            // compile the shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderCode);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "Vertex Shader"); // check if any issues with vertex shader ( if there is it will print vertex shader error)

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderCode);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, "Fragment Shader");  // check if any issues with fragment shader ( if there is it will print fragment shader error)

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShader);
            GL.AttachShader(shaderProgramHandle, fragmentShader);
            GL.LinkProgram(shaderProgramHandle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // uniforms locations
            modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            projLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");

            lightPosLoc = GL.GetUniformLocation(shaderProgramHandle, "lightPos");
            viewPosLoc = GL.GetUniformLocation(shaderProgramHandle, "viewPos");
            lightColorLoc = GL.GetUniformLocation(shaderProgramHandle, "lightColor");
            objectColorLoc = GL.GetUniformLocation(shaderProgramHandle, "objectColor");

            // load crate jpeg image
            textureHandle = LoadTexture("Assets/crate.jpeg");

            GL.UseProgram(shaderProgramHandle);
            int texLoc = GL.GetUniformLocation(shaderProgramHandle, "ourTexture");
            GL.Uniform1(texLoc, 0);

            // This make the mouse cursor invisible and captured
            CursorState = CursorState.Grabbed;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Zoom in/out
            fov -= e.OffsetY; // scroll up = zoom in, down = zoom out
            fov = MathHelper.Clamp(fov, 30f, 90f); // clamp between 30-90 degrees
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float cameraSpeed = 2.5f * (float)args.Time; // movement speed frame-rate independent

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close(); // close if click escape
            }

            // movements
            if (KeyboardState.IsKeyDown(Keys.W))
                cameraPos += cameraFront * cameraSpeed; // Forward
            if (KeyboardState.IsKeyDown(Keys.S))
                cameraPos -= cameraFront * cameraSpeed; // backward
            if (KeyboardState.IsKeyDown(Keys.A))
                cameraPos -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed; // left
            if (KeyboardState.IsKeyDown(Keys.D))
                cameraPos += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed; // right

            // what i had for assignment 5
            float lightSpeed = 1.5f * (float)args.Time;
            if (KeyboardState.IsKeyDown(Keys.Up))
                lightPos.Z -= lightSpeed;
            if (KeyboardState.IsKeyDown(Keys.Down))
                lightPos.Z += lightSpeed;
            if (KeyboardState.IsKeyDown(Keys.Left))
                lightPos.X -= lightSpeed;
            if (KeyboardState.IsKeyDown(Keys.Right))
                lightPos.X += lightSpeed;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            // stores the current position of the mouse first movement   
            if (firstMouse)
            {
                lastMousePos = new Vector2(e.X, e.Y);
                firstMouse = false;
            }
            // controls how mouse is moved either horizontally or vertically.
            float xoffset = e.X - lastMousePos.X;
            float yoffset = lastMousePos.Y - e.Y;
            lastMousePos = new Vector2(e.X, e.Y);

            // sensitivity of the movement from mouse
            float sensitivity = 0.1f;
            xoffset *= sensitivity;
            yoffset *= sensitivity;

            hori += xoffset; // left and right control
            vert += yoffset; // up and down control

            vert = MathHelper.Clamp(vert, -89f, 89f);
            // cameras direction which is forward
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(hori)) * MathF.Cos(MathHelper.DegreesToRadians(vert));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(vert));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(hori)) * MathF.Cos(MathHelper.DegreesToRadians(vert));
            cameraFront = Vector3.Normalize(front);

        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vertexArrayHandle);

            // FOV for zoom
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(fov),
                Size.X / (float)Size.Y,
                0.1f, 100f);

            // Camera view matrix
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);

            // Cube rotation all time
            float angle = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            Matrix4 model = Matrix4.CreateRotationX(angle) * Matrix4.CreateRotationY(angle * 0.1f);

            // Send matrices to shader
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            // Send light & camera info to shader
            GL.Uniform3(lightPosLoc, lightPos);
            GL.Uniform3(viewPosLoc, cameraPos);
            GL.Uniform3(lightColorLoc, lightColor);
            GL.Uniform3(objectColorLoc, objectColor);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Could not find texture file: {path}");

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);

            using (Bitmap bmp = new Bitmap(path))
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                var data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    data.Width,
                    data.Height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texId;
        }

        private void CheckShaderCompile(int shaderHandle, string name)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                Console.WriteLine($"{name} compile error: {GL.GetShaderInfoLog(shaderHandle)}");
            }
        }
    }
}