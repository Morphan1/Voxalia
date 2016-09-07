using System;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;
using LiteDB;

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

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.BLOCK_ITEM;
        }

        public override byte[] GetNetData()
        {
            byte[] phys = GetPhysicsNetData();
            int start = phys.Length;
            byte[] Data = new byte[start + 2 + 1 + 1 + 1];
            phys.CopyTo(Data, 0);
            Utilities.UshortToBytes(Original.BlockMaterial).CopyTo(Data, start);
            Data[start + 2] = Original.BlockData;
            Data[start + 3] = Original.BlockPaint;
            Data[start + 4] = Original.DamageData;
            return Data;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BLOCK_ITEM;
        }

        public override BsonDocument GetSaveData()
        {
            BsonDocument doc = new BsonDocument();
            AddPhysicsData(doc);
            doc["bie_bi"] = Original.GetItemDatum();
            return doc;
        }
        
        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block form, or destroy (or perhaps make a 'ghost') if that's not possible

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
        public override Entity Create(Region tregion, BsonDocument doc)
        {
            BlockItemEntity ent = new BlockItemEntity(tregion, BlockInternal.FromItemDatum(doc["bie_bi"].AsInt32), Location.Zero);
            ent.ApplyPhysicsData(doc);
            return ent;
        }
    }
}
