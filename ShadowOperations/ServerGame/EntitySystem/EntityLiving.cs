using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class EntityLiving: PhysicsEntity, EntityDamageable
    {
        public EntityLiving(Server tserver, bool ticks, float maxhealth)
            : base(tserver, ticks)
        {
            MaxHealth = maxhealth;
            Health = maxhealth;
        }

        public float Health = 100;

        public float MaxHealth = 100;

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
        }

        public void SetMaxHealth(float maxhealth)
        {
            MaxHealth = maxhealth;
        }
    }
}
