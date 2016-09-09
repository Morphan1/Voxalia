using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.ItemSystem;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class StaticBlockEntity : PhysicsEntity, EntityDamageable
    {
        public ItemStack Original;

        public StaticBlockEntity(Region tregion, ItemStack orig, Location pos)
            : base(tregion)
        {
            SetMass(0);
            CGroup = CollisionUtil.Item;
            Original = orig;
            Location offset;
            Shape = BlockShapeRegistry.BSD[0].GetShape(BlockDamage.NONE, out offset);
            SetPosition(pos.GetBlockLocation() + offset);
            SetOrientation(Quaternion.Identity);
        }

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.STATIC_BLOCK;
        }

        public override byte[] GetNetData()
        {
            byte[] dat = new byte[4 + 12];
            Utilities.IntToBytes((ushort)Original.Datum).CopyTo(dat, 0);
            GetPosition().ToBytes().CopyTo(dat, 4);
            return dat;
        }

        public double Health = 5;

        public double MaxHealth = 5;

        public double GetHealth()
        {
            return Health;
        }

        public double GetMaxHealth()
        {
            return MaxHealth;
        }

        public void SetHealth(double health)
        {
            Health = health;
            if (health < 0)
            {
                RemoveMe();
            }
        }

        public void SetMaxHealth(double health)
        {
            MaxHealth = health;
            if (Health > MaxHealth)
            {
                SetHealth(MaxHealth);
            }
        }

        public void Damage(double amount)
        {
            SetHealth(GetHealth() - amount);
        }
    }
}
