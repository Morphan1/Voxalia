//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ClientGame.UISystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class GameScreen: Screen
    {
        public override void Init()
        {
            // Do nothing, base init currently handles everything for some reason
        }

        public override void Tick()
        {
            // Do nothing, base tick currently handles everything for some reason
        }

        public override void SwitchTo()
        {
            MouseHandler.CaptureMouse();
        }

        public override void Render()
        {
            TheClient.renderGame();
        }
    }
}
