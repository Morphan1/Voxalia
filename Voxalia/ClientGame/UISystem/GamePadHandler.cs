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

        public static double TotalDirectionX;

        public static double TotalDirectionY;

        public static double TotalMovementX;

        public static double TotalMovementY;

        public static bool JumpKey;

        public static bool PrimaryKey;

        public static bool SecondaryKey;

        public static bool DPadUp;
        public static bool DPadDown;
        public static bool DPadLeft;
        public static bool DPadRight;

        public static bool UseKey;

        public static bool ItemLeft;

        public static bool ItemRight;

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
            DPadUp = false;
            DPadDown = false;
            DPadLeft = false;
            DPadRight = false;
            UseKey = false;
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
                    if (cap.HasAButton && state.Buttons.A == ButtonState.Pressed)
                    {
                        JumpKey = true;
                    }
                    if (cap.HasXButton && state.Buttons.X == ButtonState.Pressed)
                    {
                        UseKey = true;
                    }
                    if (cap.HasYButton && state.Buttons.Y == ButtonState.Pressed)
                    {
                        ItemLeft = true;
                    }
                    if (cap.HasBButton && state.Buttons.B == ButtonState.Pressed)
                    {
                        ItemRight = true;
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
                        DPadUp = true;
                    }
                    if (cap.HasDPadDownButton && state.DPad.Down == ButtonState.Pressed)
                    {
                        DPadDown = true;
                    }
                    if (cap.HasDPadLeftButton && state.DPad.Left == ButtonState.Pressed)
                    {
                        DPadLeft  = true;
                    }
                    if (cap.HasDPadRightButton && state.DPad.Right == ButtonState.Pressed)
                    {
                        DPadRight = true;
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
