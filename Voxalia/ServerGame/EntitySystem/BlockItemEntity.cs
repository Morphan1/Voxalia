using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockItemEntity : ModelEntity, EntityUseable
    {
        public Material Mat;

        public BlockItemEntity(World tworld, Material mat)
            : base("cube", tworld)
        {
            SetMass(5);
            CGroup = CollisionUtil.Item;
            Mat = mat;
        }

        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block

        public bool Use(Entity user)
        {
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Items.GiveItem(new ItemStack("block", TheServer, 1, "", Mat.GetName(),
                    Mat.GetDescription(), Color.White.ToArgb(), "cube", false) { Datum = (ushort)Mat });
                TheWorld.DespawnEntity(this);
                return true;
            }
            return false;
        }
    }
}
