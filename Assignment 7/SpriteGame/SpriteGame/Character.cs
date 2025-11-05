using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenTK_Sprite_Animation
{
    // --- Direction input abstraction -----------------------------------------------------------
    public enum Direction { None, Right, Left }

    // --- Character states -------------------------------------------------------------
    public enum CharacterState { Idle, Running, Jumping }

    // --- Animator -------------------------------------------------------------------------------
    public class Character
    {
        private readonly int _shader;          // Program containing uOffset/uSize
        private float _timer;                  // Accumulated time for frame stepping
        private int _frame;                    // Current frame column (0..FrameCount-1)
        private CharacterState _state;         // Current FSM state
        private Direction _lastDirection;      // Last non-none direction

        public Direction LastDirection => _lastDirection;

        // Timing
        private const float FrameTime = 0.15f; // seconds per frame
        private const int FrameCount = 4;      // frames per row

        // Sprite sheet layout (pixel units)
        private const float FrameW = 64f;
        private const float FrameH = 128f;
        private const float Gap = 60f;          // horizontal spacing between frames
        private const float TotalW = FrameW + Gap;
        private const float SheetW = 4 * TotalW - Gap; // 4 columns
        private const float SheetH = 256f;     // 2 rows of 128 => 256

        private bool _looping; // new field

        public Character(int shader)
        {
            _shader = shader;
            _state = CharacterState.Idle;
            _lastDirection = Direction.Right;
            SetFrame(0, 0); // Start on idle frame
        }

        // Call this from Game.cs to change the state
        public void SetState(CharacterState state, Direction inputDir)
        {
            if (_state == state && inputDir == _lastDirection)
                return;

            _state = state;
            if (inputDir != Direction.None)
                _lastDirection = inputDir;

            _frame = 0;
            _timer = 0f;

            // Only Running (or Walk) should loop
            _looping = _state == CharacterState.Running;
        }


        // Call this every frame to update animation based on current state
        public void UpdateAnimation(float delta)
        {
            switch (_state)
            {
                case CharacterState.Idle:
                    SetFrame(0, _lastDirection == Direction.Right ? 0 : 1);
                    break;

                case CharacterState.Running:
                    _timer += delta;
                    if (_timer >= FrameTime)
                    {
                        _timer -= FrameTime;

                        if (_looping)
                            _frame = (_frame + 1) % FrameCount;  // looping animation
                        else
                            _frame = Math.Min(_frame + 1, FrameCount - 1); // stop at last frame
                    }
                    SetFrame(_frame, _lastDirection == Direction.Right ? 0 : 1);
                    break;

                case CharacterState.Jumping:
                    SetFrame(0, _lastDirection == Direction.Right ? 0 : 1); // single jump frame
                    break;
            }
        }


        public void Render()
        {
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        // Converts (col,row) in pixels to normalized UVs and uploads to shader
        private void SetFrame(int col, int row)
        {
            float x = (col * TotalW) / SheetW;
            float y = (row * FrameH) / SheetH;
            float w = FrameW / SheetW;
            float h = FrameH / SheetH;

            GL.UseProgram(_shader);
            int off = GL.GetUniformLocation(_shader, "uOffset");
            int sz = GL.GetUniformLocation(_shader, "uSize");
            GL.Uniform2(off, x, y);
            GL.Uniform2(sz, w, h);
        }
    }
}

