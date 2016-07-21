using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace Voxalia.ClientGame.UISystem
{
    public class GamePadHandler
    {
        public static void Init()
        {
        }

        public static float SprintOrWalk;
        
        public static double TotalDirectionX;

        public static double TotalDirectionY;

        public static double TotalMovementX;

        public static double TotalMovementY;

        public static bool JumpKey;

        public static bool PrimaryKey;

        public static bool SecondaryKey;
        
        public static bool ChangeLeft;

        public static bool ChangeRight;

        public static bool UseKey;

        public static bool ItemLeft;

        public static bool ItemRight;

        public static bool ItemUp;

        public static bool ItemDown;

        public static bool ReloadKey;

        public static void Tick(double delta)
        {
            TotalDirectionX = 0;
            TotalDirectionY = 0;
            TotalMovementX = 0;
            TotalMovementY = 0;
            JumpKey = false;
            PrimaryKey = false;
            SecondaryKey = false;
            ItemLeft = false;
            ItemRight = false;
            ItemUp = false;
            ItemDown = false;
            ChangeLeft = false;
            ChangeRight = false;
            UseKey = false;
            ReloadKey = false;
            SprintOrWalk = 0f;
            for (int i = 0; i < 4; i++)
            {
                GamePadCapabilities cap = GamePad.GetCapabilities(i);
                if (cap.IsConnected)
                {
                    GamePadState state = GamePad.GetState(i);
                    if (cap.HasRightXThumbStick)
                    {
                        TotalDirectionX -= state.ThumbSticks.Right.X;
                    }
                    if (cap.HasRightYThumbStick)
                    {
                        TotalDirectionY += state.ThumbSticks.Right.Y;
                    }
                    if (cap.HasLeftXThumbStick)
                    {
                        TotalMovementX += state.ThumbSticks.Left.X;
                    }
                    if (cap.HasLeftYThumbStick)
                    {
                        TotalMovementY += state.ThumbSticks.Left.Y;
                    }
                    if (cap.HasLeftTrigger)
                    {
                        SprintOrWalk -= state.Triggers.Left;
                    }
                    if (cap.HasRightTrigger)
                    {
                        SprintOrWalk += state.Triggers.Right;
                    }
                    if (cap.HasAButton && state.Buttons.A == ButtonState.Pressed)
                    {
                        ChangeLeft = true;
                    }
                    if (cap.HasBButton && state.Buttons.B == ButtonState.Pressed)
                    {
                        ChangeRight = true;
                    }
                    if (cap.HasXButton && state.Buttons.X == ButtonState.Pressed)
                    {
                        UseKey = true;
                    }
                    if (cap.HasYButton && state.Buttons.Y == ButtonState.Pressed)
                    {
                        ReloadKey = true;
                    }
                    if (cap.HasLeftShoulderButton && state.Buttons.LeftShoulder == ButtonState.Pressed)
                    {
                        SecondaryKey = true;
                    }
                    if (cap.HasRightShoulderButton && state.Buttons.RightShoulder == ButtonState.Pressed)
                    {
                        PrimaryKey = true;
                    }
                    if (cap.HasDPadUpButton && state.DPad.Up == ButtonState.Pressed)
                    {
                        ItemUp = true;
                    }
                    if (cap.HasDPadDownButton && state.DPad.Down == ButtonState.Pressed)
                    {
                        ItemDown = true;
                    }
                    if (cap.HasDPadLeftButton && state.DPad.Left == ButtonState.Pressed)
                    {
                        ItemLeft = true;
                    }
                    if (cap.HasDPadRightButton && state.DPad.Right == ButtonState.Pressed)
                    {
                        ItemRight = true;
                    }
                    if (cap.HasLeftStickButton && state.Buttons.LeftStick == ButtonState.Pressed)
                    {
                        JumpKey = true;
                    }
                }
            }
            if (TotalDirectionX < -1)
            {
                TotalDirectionX = -1;
            }
            if (TotalDirectionX > 1)
            {
                TotalDirectionX = 1;
            }
            if (TotalDirectionY < -1)
            {
                TotalDirectionY = -1;
            }
            if (TotalDirectionY > 1)
            {
                TotalDirectionY = 1;
            }
            if (SprintOrWalk > 1)
            {
                SprintOrWalk = 1;
            }
            if (SprintOrWalk < -1)
            {
                SprintOrWalk = -1;
            }
            if (Math.Abs(TotalDirectionX) < 0.05f)
            {
                TotalDirectionX = 0;
            }
            if (Math.Abs(TotalDirectionY) < 0.05f)
            {
                TotalDirectionY = 0;
            }
        }
    }
}
