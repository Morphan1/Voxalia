using System;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockItemEntity : PhysicsEntity, EntityUseable, EntityDamageable
    {
        public Material Mat;
        public byte Dat;

        public BlockItemEntity(Region tregion, Material mat, byte dat, Location pos)
            : base(tregion)
        {
            SetMass(5);
            CGroup = CollisionUtil.Item;
            Dat = dat;
            Location offset;
            Shape = BlockShapeRegistry.BSD[dat].GetShape(out offset);
            SetPosition(pos.GetBlockLocation() + offset);
            Mat = mat;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BLOCK_ITEM;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 3];
            bbytes.CopyTo(res, 0);
            Utilities.UshortToBytes((ushort)Mat).CopyTo(res, bbytes.Length);
            res[bbytes.Length + 2] = Dat;
            return res;
        }

        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block

        /// <summary>
        /// Gets the itemstack this block represents.
        /// </summary>
        public ItemStack GetItem()
        {
            // TODO: Proper texture / model / ...
            return new ItemStack("block", Mat.ToString(), TheServer, 1, "", Mat.GetName(),
                    "A standard block of " + Mat.ToString(), System.Drawing.Color.White, "cube", false) { Datum = (ushort)Mat };
        }

        public bool Use(Entity user)
        {
            if (Removed)
            {
                return false;
            }
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Items.GiveItem(GetItem());
                RemoveMe();
                return true;
            }
            return false;
        }

        public float Health = 5;

        public float MaxHealth = 5;

        public float GetHealth()
        {
            return Health;
        }

        public float GetMaxHealth()
        {
            return MaxHealth;
        }

        public void SetHealth(float health)
        {
            Health = health;
            if (health < 0)
            {
                RemoveMe();
            }
        }

        public void SetMaxHealth(float health)
        {
            MaxHealth = health;
            if (Health > MaxHealth)
            {
                SetHealth(MaxHealth);
            }
        }

        public void Damage(float amount)
        {
            SetHealth(GetHealth() - amount);
        }
    }

    public class BlockItemEntityConstructor: EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4;
            ushort tmat = Utilities.BytesToUshort(Utilities.BytesPartial(input, plen, 2));
            byte tdat = input[plen + 2];
            BlockItemEntity ent = new BlockItemEntity(tregion, (Material)tmat, tdat, Location.Zero);
            ent.ApplyBytes(input);
            return ent;
        }
    }
}
