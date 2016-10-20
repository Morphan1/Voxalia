//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public abstract class UIMenuItem
    {
        public UIScreen Menus;

        public abstract void MouseEnter();

        public abstract void MouseLeave();

        public abstract void MouseLeftDown();

        public abstract void MouseLeftUp();

        public abstract void Init();

        public abstract void Render(double delta, int xoff, int yoff);

        public abstract bool Contains(int x, int y);

        public bool HoverInternal = false;

        public virtual void Tick(double delta)
        {
        }

        public virtual void MouseLeftDownOutside()
        {

        }
    }
}
