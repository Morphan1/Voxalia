using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class PistolGunItem: BaseGunItem
    {
        public PistolGunItem()
            : base("pistol_gun", 0.03f, 10f, 0f, 0f, 10f, 7, "9mm_ammo", 0.5f, 1, -1f)
        {
        }
    }
}
