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
}
