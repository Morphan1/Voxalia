//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class GameScreen : UIScreen
    {
        public GameScreen(Client tclient) : base(tclient)
        {
            ResetOnRender = false;
        }

        public override void SwitchTo()
        {
            MouseHandler.CaptureMouse();
        }
        
        protected override void Render(double delta, int xoff, int yoff)
        {
            TheClient.renderGame();
            TheClient.Establish2D();
            TheClient.Render2DGame();
        }
    }
}
