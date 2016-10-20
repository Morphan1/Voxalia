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

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIScreen : UIElement
    {
        public Client TheClient;

        public UIScreen(Client tclient) : base(UIAnchor.TOP_LEFT, () => tclient.Window.Width, () => tclient.Window.Height, () => 0, () => 0)
        {
            TheClient = tclient;
        }

        public override Client GetClient()
        {
            return TheClient;
        }

        private bool pDown;

        protected override void TickChildren(double delta)
        {
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
                    }
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDown(mX, mY);
                    }
                }
                else if (element.HoverInternal)
                {
                    element.HoverInternal = false;
                    element.MouseLeave(mX, mY);
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDownOutside(mX, mY);
                    }
                }
                else if (mDown && !pDown)
                {
                    element.MouseLeftDownOutside(mX, mY);
                }
                element.FullTick(TheClient.Delta);
            }
            pDown = mDown;
        }
    }
}
