//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class RifleGunItem : BaseGunItem
    {
        public RifleGunItem()
            : base("rifle_gun", 0.03f, 10f, 0f, 0f, 250f, 30, "rifle_ammo", 2, 1, 0.099f, 2, false)
        {
        }
    }
}
