using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class StaticBlockEntity : BlockItemEntity
    {
        public StaticBlockEntity(Region tregion, Material mat, byte paint)
            : base(tregion, mat, 0, paint, BlockDamage.NONE)
        {
            SetMass(0);
        }
    }
}
