//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ClientGame.ClientMainSystem
{
    public abstract class Screen
    {
        public Client TheClient;

        public abstract void Tick();

        public abstract void Render();

        public abstract void Init();

        public abstract void SwitchTo();
    }
}
