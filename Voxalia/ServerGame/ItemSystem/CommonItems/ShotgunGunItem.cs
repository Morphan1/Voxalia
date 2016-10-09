//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class ShotgunGunItem: BaseGunItem
    {
        public ShotgunGunItem()
            : base("shotgun_gun", 0.03f, 10f, 0f, 0f, 70f, 8, "shotgun_ammo", 10, 5, 0.5f, 2, true)
        {
        }
    }
}
