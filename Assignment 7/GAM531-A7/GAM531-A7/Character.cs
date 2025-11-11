using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTK_Sprite_Animation
{
    public enum Direction { None, Right, Left }
    public enum CharacterState { Idle, Running, Jumping }

    public class Character
    {
        private readonly int _shader;
        private float _timer;
        private int _frame;
        private CharacterState _state;
        private Direction _lastDirection;
        private bool _looping;

        // texture IDS
        private int _idleTex;
        private int _runTex;
        private int _jumpTex;

        public Direction LastDirection => _lastDirection;

        private const float FrameTime = 0.15f;
        private const int FrameCount = 4;

        private const float FrameW = 64f;
        private const float FrameH = 128f;
        private const float Gap = 60f;
        private const float TotalW = FrameW + Gap;
        private const float SheetW = 4 * TotalW - Gap;
        private const float SheetH = 256f;

        // Layouts for each animation
        private struct SpriteLayout
        {
            public int FrameCount;
            public float FrameW;
            public float FrameH;
            public float SheetW;
            public float SheetH;

            public SpriteLayout(int frames, float fw, float fh, float sw, float sh)
            {
                FrameCount = frames;
                FrameW = fw;
                FrameH = fh;
                SheetW = sw;
                SheetH = sh;
            }
        }

        // Layouts for animation
        private SpriteLayout _idleLayout;
        private SpriteLayout _runLayout;
        private SpriteLayout _jumpLayout;
        private SpriteLayout _currentLayout;


        public Character(int shader)
        {
            _shader = shader;
            _state = CharacterState.Idle;
            _lastDirection = Direction.Right;
            SetFrame(0, 0);
        }

        // load texture externally
        public void LoadTextures(int idleTex, int runTex, int jumpTex)
        {
            _idleTex = idleTex;
            _runTex = runTex;
            _jumpTex = jumpTex;

            _idleLayout = new SpriteLayout(4, 64f, 128f, 256f, 256f);    // Original sheet
            _runLayout = new SpriteLayout(6, 128f, 128f, 768f, 128f);   // Owlet_Run_6.png
            _jumpLayout = new SpriteLayout(8, 128f, 128f, 1024f, 128f);  // Owlet_Jump_8.png

            _currentLayout = _idleLayout; // start on idle
        }


        public void SetState(CharacterState state, Direction inputDir)
        {
            if (_state == state && inputDir == _lastDirection)
                return;

            _state = state;

            _currentLayout = _state switch
            {
                CharacterState.Running => _runLayout,
                CharacterState.Jumping => _jumpLayout,
                _ => _idleLayout
            };

            if (inputDir != Direction.None)
                _lastDirection = inputDir;

            _frame = 0;
            _timer = 0f;
            _looping = _state == CharacterState.Running;
        }

        public void UpdateAnimation(float delta)
        {
            switch (_state)
            {
                case CharacterState.Idle:
                    SetFrame(_frame, 0);
                    break;

                case CharacterState.Running:
                    _timer += delta;
                    if (_timer >= FrameTime)
                    {
                        _timer -= FrameTime;
                        _frame = _looping ? (_frame + 1) % FrameCount : Math.Min(_frame + 1, FrameCount - 1);
                    }
                    SetFrame(_frame, 0);
                    break;

                case CharacterState.Jumping:
                    SetFrame(_frame, 0);
                    break;
            }
        }

        // binds the correct sprite sheet before drawing
        public void Render()
        {
            int flipLoc = GL.GetUniformLocation(_shader, "flipX");
            GL.Uniform1(flipLoc, _lastDirection == Direction.Right ? 1.0f : -1.0f);

            switch (_state)
            {
                case CharacterState.Running:
                    GL.BindTexture(TextureTarget.Texture2D, _runTex);
                    break;
                case CharacterState.Jumping:
                    GL.BindTexture(TextureTarget.Texture2D, _jumpTex);
                    break;
                default:
                    GL.BindTexture(TextureTarget.Texture2D, _idleTex);
                    break;
            }

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }


        private void SetFrame(int col, int row)
        {
            float x = (col * _currentLayout.FrameW) / _currentLayout.SheetW;
            float y = (row * _currentLayout.FrameH) / _currentLayout.SheetH;
            float w = _currentLayout.FrameW / _currentLayout.SheetW;
            float h = _currentLayout.FrameH / _currentLayout.SheetH;

            GL.UseProgram(_shader);
            int off = GL.GetUniformLocation(_shader, "uOffset");
            int sz = GL.GetUniformLocation(_shader, "uSize");
            GL.Uniform2(off, x, y);
            GL.Uniform2(sz, w, h);
        }

    }
}
