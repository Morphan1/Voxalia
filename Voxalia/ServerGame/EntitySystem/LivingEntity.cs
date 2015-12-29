using System;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class LivingEntity: PhysicsEntity, EntityDamageable
    {
        public LivingEntity(Region tregion, float maxhealth)
            : base(tregion)
        {
            MaxHealth = maxhealth;
            Health = maxhealth;
        }

        public float Health = 100;

        public float MaxHealth = 100;

        public virtual float GetHealth()
        {
            return Health;
        }

        public virtual float GetMaxHealth()
        {
            return MaxHealth;
        }

        public virtual void SetHealth(float health)
        {
            Health = Math.Min(health, MaxHealth);
            if (MaxHealth != 0 && Health <= 0)
            {
                Die();
            }
        }

        public virtual void Damage(float amount)
        {
            SetHealth(GetHealth() - amount);
        }

        public virtual void SetMaxHealth(float maxhealth)
        {
            MaxHealth = maxhealth;
        }

        public abstract void Die();
    }
}
