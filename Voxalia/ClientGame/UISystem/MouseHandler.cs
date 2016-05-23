using System;
using Voxalia.Shared;
using OpenTK.Input;
using Voxalia.ClientGame.ClientMainSystem;
using System.Drawing;


namespace Voxalia.ClientGame.UISystem
{
    /// <summary>
    /// Handles mouse input.
    /// TODO: Make non-static
    /// </summary>
    public class MouseHandler
    {
        /// <summary>
        /// Whether the mouse is captured.
        /// </summary>
        public static bool MouseCaptured = false;

        /// <summary>
        /// How much the mouse has moved this tick.
        /// </summary>
        public static Location MouseDelta = new Location();

        /// <summary>
        /// The current mouse state for this tick.
        /// </summary>
        public static MouseState CurrentMouse;

        /// <summary>
        /// The mouse state during the previous tick.
        /// </summary>
        public static MouseState PreviousMouse;

        /// <summary>
        /// How much the mouse was scrolled this tick.
        /// </summary>
        public static int MouseScroll = 0;

        public static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        /// <summary>
        /// Captures the mouse to this window.
        /// </summary>
        public static void CaptureMouse()
        {
            if (UIConsole.Open)
            {
                UIConsole.MouseWasCaptured = true;
                return;
            }
            CenterMouse();
            MouseCaptured = true;
            if (IsWindows)
            {
                Client.Central.Window.CursorVisible = false;
            }
        }

        /// <summary>
        /// Uncaptures the mouse for this window.
        /// </summary>
        public static void ReleaseMouse()
        {
            MouseCaptured = false;
            Client.Central.Window.CursorVisible = true;
        }

        /// <summary>
        /// Moves the mouse back to the center position.
        /// </summary>
        public static void CenterMouse()
        {
            Point center = Client.Central.Window.PointToScreen(new Point(Client.Central.Window.Width / 2, Client.Central.Window.Height / 2));
            Mouse.SetPosition(center.X, center.Y);
            mx = (Client.Central.Window.Width / 2);
            my = (Client.Central.Window.Height / 2);
        }

        public static float pwheelstate;
        public static float cwheelstate;

        static int mx;
        static int my;

        public static void Window_MouseMove(object sender, MouseMoveEventArgs e)
        {
            mx = e.X;
            my = e.Y;
        }

        public static int MouseX()
        {
            return mx;
        }

        public static int MouseY()
        {
            return my;
        }

        /// <summary>
        /// Updates mouse movement.
        /// </summary>
        public static void Tick()
        {
            if (Client.Central.Window.Focused && MouseCaptured && !UIConsole.Open)
            {
                double MoveX = (((Client.Central.Window.Width / 2) - MouseX()) * Client.Central.CVars.u_mouse_sensitivity.ValueD);
                double MoveY = (((Client.Central.Window.Height / 2) - MouseY()) * Client.Central.CVars.u_mouse_sensitivity.ValueD);
                MouseDelta = new Location(MoveX, MoveY, 0);
                CenterMouse();
                PreviousMouse = CurrentMouse;
                CurrentMouse = Mouse.GetState();
                pwheelstate = cwheelstate;
                cwheelstate = CurrentMouse.WheelPrecise;
                MouseScroll = (int)(cwheelstate - pwheelstate);
            }
            else
            {
                MouseDelta = Location.Zero;
            }
            if (Client.Central.Window.Focused && !MouseCaptured)
            {
                PreviousMouse = CurrentMouse;
                CurrentMouse = Mouse.GetState();
                pwheelstate = cwheelstate;
                cwheelstate = CurrentMouse.WheelPrecise;
                MouseScroll = (int)(cwheelstate - pwheelstate);
            }
            if (!Client.Central.Window.Focused)
            {
                cwheelstate = Mouse.GetState().WheelPrecise;
                pwheelstate = cwheelstate;
            }
        }
    }
}
