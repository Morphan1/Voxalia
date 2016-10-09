//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class PistolGunItem: BaseGunItem
    {
        public PistolGunItem()
            : base("pistol_gun", 0.03f, 10f, 0f, 0f, 90f, 7, "9mm_ammo", 0.5f, 1, 0.1f, 1, true)
        {
        }
    }
}
