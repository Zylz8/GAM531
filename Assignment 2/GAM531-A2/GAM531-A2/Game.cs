using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;

        private int modelLoc, viewLoc, projLoc;


        // Arrays to store transformation state for each triangle
        private float[] rotationAngles;
        private float[] scaleFactors;
        private bool[] scalingUp;

        private const int TriangleCount = 1;

        // Constructor
        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            // Set window size to 1280x768
            this.Size = new Vector2i(1280, 768);

            // Center the window on the screen
            this.CenterWindow(this.Size);
        }

        // Called automatically whenever the window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the OpenGL viewport to match the new window dimensions
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        // Called once when the game starts, ideal for loading resources
        protected override void OnLoad()
        {
            base.OnLoad();

            // Set the background color (RGBA)
            GL.ClearColor(new Color4(0.2f, 0.7f, 0.2f, 1f)); // Green Background

            // Define a simple triangle in normalized device coordinates (NDC)
            float[] vertices = new float[]
            {
                // Triangle 1
                0.5f,  0.5f, 0.0f,   // Top vertex
               -0.5f, -0.5f, 0.0f,   // Bottom-left vertex
                0.5f, -0.5f, 0.0f,    // Bottom-right vertex

                // Triangle 2
                0.5f, 0.5f, 0.0f, // Top Right
                -0.5f, 0.5f, 0.0f, // Top Left
                -0.5f, -0.5f, 0.0f // Bottom Right
            };

            // Generate a Vertex Buffer Object (VBO) to store vertex data on GPU
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind to prevent accidental modifications

            // Generate a Vertex Array Object (VAO) to store the VBO configuration
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            // Bind the VBO and define the layout of vertex data for shaders
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // Vertex shader: positions each vertex
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition; // Vertex position input
                
                uniform mat4 model; // this is the transformation matrix
                uniform mat4 view;
                uniform mat4 projection;
                
                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0); // Convert vec3 to vec4 for output and apply transformation
                }
            ";

            // Fragment shader: outputs a single color
            string fragmentShaderCode = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(1.0f, 0.2f, 0.1f, 1.0f); // Orange-red color
                }
            ";

            // Compile shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            // Create shader program and link shaders
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            // Cleanup shaders after linking (no longer needed individually)
            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            // The uniform locations
            modelLoc = GL.GetUniformLocation(shaderProgramHandle, "model");
            viewLoc = GL.GetUniformLocation(shaderProgramHandle, "view");
            projLoc = GL.GetUniformLocation(shaderProgramHandle, "projection");

            // Initialize transformation arrays for multiple triangles
            rotationAngles = new float[TriangleCount];
            scaleFactors = new float[TriangleCount];
            scalingUp = new bool[TriangleCount];

            for (int i = 0; i < TriangleCount; i++)
            {
                rotationAngles[i] = i * 0.5f; // stagger initial rotations
                scaleFactors[i] = 1f;
                scalingUp[i] = true;
            }
        }

        // Called every frame to update game logic
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // Update rotation and scale for each triangle independently
            for (int i = 0; i < TriangleCount; i++)
            {
                // Rotate continuously
                rotationAngles[i] += (float)args.Time * (i + 1); // different speed for each triangle

                // Oscillating scale between 0.5 and 1.5
                if (scalingUp[i])
                {
                    scaleFactors[i] += (float)args.Time;
                    if (scaleFactors[i] >= 1.5f) scalingUp[i] = false;
                }
                else
                {
                    scaleFactors[i] -= (float)args.Time;
                    if (scaleFactors[i] <= 0.5f) scalingUp[i] = true;
                }
            }
        }

        // Called every frame to render graphics
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            // Use our shader program
            GL.UseProgram(shaderProgramHandle);

            // View matrix (camera looking at origin)
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.UnitY);

            // Projection matrix (perspective)
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                (float)Size.X / Size.Y,
                0.1f,
                100f
            );

            // Send view and projection to shader (same for all triangles)
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.BindVertexArray(vertexArrayHandle);

            for (int i = 0; i < TriangleCount; i++)
            {
                // Rotation quaternion for this triangle
                Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitY, rotationAngles[i]);
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);

                // Scaling
                Matrix4 scaleMatrix = Matrix4.CreateScale(scaleFactors[i]);

                // Translation: spread triangles along X axis
                Matrix4 translationMatrix = Matrix4.CreateTranslation(-2f + i * 2f, 0f, 0f);

                // Combine transformations: Model = Translation * Rotation * Scale
                Matrix4 model = scaleMatrix * rotationMatrix * translationMatrix;

                // Send model matrix to shader
                GL.UniformMatrix4(modelLoc, false, ref model);

                // Draw the rectangle using 2 triangles
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }


            // Bind the VAO
            GL.BindVertexArray(0);

            // Display the rendered frame
            SwapBuffers();
        }

        // Called when the game is closing or resources need to be released
        protected override void OnUnload()
        {
            // Unbind and delete buffers and shader program
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();
        }

        // Helper function to check for shader compilation errors
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