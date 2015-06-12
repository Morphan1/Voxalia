using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class RifleGunItem : BaseGunItem
    {
        public RifleGunItem()
            : base("rifle_gun", 0.03f, 10f, 0f, 0f, 90f, 30, "rifle_ammo", 2, 1, 0.08f, 2, false)
        {
        }
    }
}
