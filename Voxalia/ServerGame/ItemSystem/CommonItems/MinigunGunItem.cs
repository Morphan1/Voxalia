using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class MinigunGunItem : BaseGunItem
    {
        public MinigunGunItem()
            : base("minigun_gun", 0.03f, 10f, 0f, 0f, 90f, 1000, "minigun_ammo", 4, 1, 0.049f, 2, false)
        {
        }
    }
}
