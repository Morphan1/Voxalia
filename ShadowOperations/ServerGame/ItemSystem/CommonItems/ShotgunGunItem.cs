using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class ShotgunGunItem: BaseGunItem
    {
        public ShotgunGunItem()
            : base("shotgun_gun", 0.03f, 10f, 0f, 0f, 10f, 7, "shotgun_ammo", 10, 5, -1f)
        {
        }
    }
}
