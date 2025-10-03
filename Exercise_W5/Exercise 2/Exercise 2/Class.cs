using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace WindowEngine
{
    public class Game
    {
        private readonly Surface screen;
        private int texture;
        private int vertexArrayHandle;
        private int vertexBufferHandle;

        private int shaderProgramHandle;
        private int count = 0;

        public Game(int width, int height)
        {
            screen = new Surface(width, height);
        }

        public void Init()
        {
            // Set clear color
            GL.ClearColor(0f, 0f, 0f, 1f);

            // Disable depth testing/culling for 2D
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            // Generate texture
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screen.width, screen.height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            float[] vertices = {
                // Positions       // TexCoords
                -0.5f,  0.5f, 0.0f,     0.0f, 1.0f,  // Top-left
                 0.5f,  0.5f, 0.0f,     1.0f, 1.0f,  // Top-right
                -0.5f, -0.5f, 0.0f,     0.0f, 0.0f,  // Bottom-left
                 0.5f, -0.5f, 0.0f,     1.0f, 0.0f   // Bottom-right
};


            vertexArrayHandle = GL.GenVertexArray();
            vertexBufferHandle = GL.GenBuffer();
            GL.BindVertexArray(vertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Shaders
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPos;
                layout(location = 1) in vec2 aTex;
                out vec2 TexCoords;
                void main() {
                    gl_Position = vec4(aPos, 1.0);
                    TexCoords = aTex;
                }";

            string fragmentShaderCode = @"
                #version 330 core
                in vec2 TexCoords;
                out vec4 FragColor;
                uniform sampler2D uTexture;
                void main() {
                    FragColor = texture(uTexture, TexCoords);
                }";

            int vShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShaderHandle, vertexShaderCode);
            GL.CompileShader(vShaderHandle);
            CheckShaderError(vShaderHandle, "Vertex Shader");

            int vFragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vFragmentHandle, fragmentShaderCode);
            GL.CompileShader(vFragmentHandle);
            CheckShaderError(vFragmentHandle, "Fragment Shader");

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vShaderHandle);
            GL.AttachShader(shaderProgramHandle, vFragmentHandle);
            GL.LinkProgram(shaderProgramHandle);
            CheckProgramError(shaderProgramHandle);

            GL.DeleteShader(vShaderHandle);
            GL.DeleteShader(vFragmentHandle);
        }

        public void Tick()
        {
            count++;

            for (int y = 0; y < screen.height; y++)
            {
                for (int x = 0; x < screen.width; x++)
                {
                    int r = (int)((x / (float)screen.width) * 255);
                    int g = (int)((y / (float)screen.height) * 255);
                    int b = (int)((Math.Sin(count * 0.05) + 1) / 2 * 255); // fades 0-255

                    int index = (y * screen.width + x) * 4;
                    screen.pixels[index + 0] = (byte)b;   // Blue
                    screen.pixels[index + 1] = (byte)g;   // Green
                    screen.pixels[index + 2] = (byte)r;   // Red
                    screen.pixels[index + 3] = 255;       // Alpha
                }
            }


            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, screen.width, screen.height,
                             PixelFormat.Bgra, PixelType.UnsignedByte, screen.pixels);

            // Render
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public void Cleanup()
        {
            GL.DeleteBuffer(vertexBufferHandle);
            GL.DeleteVertexArray(vertexArrayHandle);
            GL.DeleteProgram(shaderProgramHandle);
            GL.DeleteTexture(texture);
        }

        private void CheckShaderError(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"{name} compilation error: {info}");
            }
        }

        private void CheckProgramError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Program link error: {info}");
            }
        }
    }

    public class Surface
    {
        public byte[] pixels;
        public int width, height;

        public Surface(int width, int height)
        {
            this.width = width;
            this.height = height;
            pixels = new byte[width * height * 4];
        }
    }

}
