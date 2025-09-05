using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;


namespace FirstOpenTK {
    public class Game : GameWindow {
        // Constructor for the Game Class
        public Game() 
            : base(GameWindowSettings.Default, NativeWindowSettings.Default) 
        {
            this.CenterWindow(new Vector2i(1280, 768));    // assure our window is centered in the middle then the size 1280/768
        }
        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        // Called every frame to update game logic, physics, or input handling
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        // Called when I need to update any game visuals
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.ClearColor(new Color4(2f, 0.1f, 0.1f, 1f)); // colors (float colors between 0. and 1)
            GL.Clear(ClearBufferMask.ColorBufferBit); 
            SwapBuffers();
        }
    }
}