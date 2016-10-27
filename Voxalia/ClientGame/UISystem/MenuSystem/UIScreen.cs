//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK.Input;
using System;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIScreen : UIElement
    {
        public Client TheClient;

        public UIScreen(Client tclient) : base(UIAnchor.TOP_LEFT, () => 0, () => 0, () => 0, () => 0)
        {
            TheClient = tclient;
            Width = () => Parent == null ? TheClient.Window.Width : Parent.GetWidth();
            Height = () => Parent == null ? TheClient.Window.Height : Parent.GetHeight();
        }

        public override Client GetClient()
        {
            return TheClient;
        }

        private bool pDown;

        protected override void TickChildren(double delta)
        {
            if (Parent != null)
            {
                base.TickChildren(delta);
                return;
            }
            int mX = MouseHandler.MouseX();
            int mY = MouseHandler.MouseY();
            bool mDown = MouseHandler.CurrentMouse.IsButtonDown(MouseButton.Left);
            foreach (UIElement element in Children)
            {
                if (element.Contains(mX, mY))
                {
                    if (!element.HoverInternal)
                    {
                        element.HoverInternal = true;
                        element.MouseEnter(mX, mY);
                        SysConsole.Output(OutputType.DEBUG, "Enter!");
                    }
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDown(mX, mY);
                        SysConsole.Output(OutputType.DEBUG, "LDown!");
                    }
                    else if (!mDown && pDown)
                    {
                        element.MouseLeftUp(mX, mY);
                        SysConsole.Output(OutputType.DEBUG, "LUp!");
                    }
                }
                else if (element.HoverInternal)
                {
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDownOutside(mX, mY);
                        SysConsole.Output(OutputType.DEBUG, "LDownOutside!");
                    }
                    element.HoverInternal = false;
                    element.MouseLeave(mX, mY);
                    SysConsole.Output(OutputType.DEBUG, "Leave!");
                }
                else if (mDown && !pDown)
                {
                    element.MouseLeftDownOutside(mX, mY);
                    SysConsole.Output(OutputType.DEBUG, "LDownOutside!");
                }
                element.FullTick(TheClient.Delta);
            }
            pDown = mDown;
        }

        protected override void RenderChildren(double delta, int xoff, int yoff)
        {
            TheClient.Establish2D();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0.5f, 0.5f, 1 });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1 });
            base.RenderChildren(delta, xoff, yoff);
        }

        public virtual void SwitchTo()
        {
        }
    }
}
