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
        public BlockInternal Original;

        public BlockItemEntity(Region tregion, BlockInternal orig, Location pos)
            : base(tregion)
        {
            SetMass(5);
            CGroup = CollisionUtil.Item;
            Original = orig;
            Location offset;
            Shape = BlockShapeRegistry.BSD[orig.BlockData].GetShape(orig.Damage, out offset);
            SetPosition(pos.GetBlockLocation() + offset);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BLOCK_ITEM;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 4];
            bbytes.CopyTo(res, 0);
            Utilities.UshortToBytes(Original.BlockMaterial).CopyTo(res, bbytes.Length);
            res[bbytes.Length + 2] = Original.BlockData;
            res[bbytes.Length + 3] = Original.BlockPaint;
            return res;
        }

        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block form, or destroy if that's not possible

        /// <summary>
        /// Gets the itemstack this block represents.
        /// </summary>
        public ItemStack GetItem()
        {
            ItemStack its = TheServer.Items.GetItem("blocks/" + ((Material)Original.BlockMaterial).ToString());
            its.Datum = Original.GetItemDatum();
            return its;
        }

        public void StartUse(Entity user)
        {
            if (!Removed)
            {
                if (user is PlayerEntity)
                {
                    ((PlayerEntity)user).Items.GiveItem(GetItem());
                    RemoveMe();
                }
            }
        }

        public void StopUse(Entity user)
        {
            // Do nothing.
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
            int plen = PhysicsEntity.PhysByteLen;
            ushort tmat = Utilities.BytesToUshort(Utilities.BytesPartial(input, plen, 2));
            byte tdat = input[plen + 2];
            byte tpaint = input[plen + 3];
            BlockItemEntity ent = new BlockItemEntity(tregion, new BlockInternal(tmat, tdat, tpaint, 0), Location.Zero);
            ent.ApplyBytes(input);
            return ent;
        }
    }
}
