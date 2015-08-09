using System.Drawing;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockItemEntity : PhysicsEntity, EntityUseable
    {
        public Material Mat;
        public byte Dat;

        public BlockItemEntity(WorldSystem.Region tregion, Material mat, byte dat, Location pos)
            : base(tregion, true)
        {
            SetMass(5);
            CGroup = CollisionUtil.Item;
            Dat = dat;
            Location offset;
            Shape = BlockShapeRegistry.BSD[dat].GetShape(out offset);
            SetPosition(pos.GetBlockLocation() + offset);
            Mat = mat;
        }

        public bool pActive = false;

        public double deltat = 0;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            if (!pActive && GetMass() > 0)
            {
                deltat += TheRegion.Delta;
                if (deltat > 2.0)
                {
                    TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
                }
            }
            base.Tick();
        }

        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block

        /// <summary>
        /// Gets the itemstack this block represents.
        /// </summary>
        public ItemStack GetItem()
        {
            return new ItemStack("block", Mat.ToString(), TheServer, 1, "", Mat.GetName(),
                    Mat.GetDescription(), Color.White.ToArgb(), "cube", false) { Datum = (ushort)Mat };
        }

        public bool Use(Entity user)
        {
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Items.GiveItem(GetItem());
                TheRegion.DespawnEntity(this);
                return true;
            }
            return false;
        }
    }
}
