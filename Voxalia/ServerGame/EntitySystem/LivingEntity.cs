using System;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class LivingEntity: PhysicsEntity, EntityDamageable
    {
        public LivingEntity(Region tregion, double maxhealth)
            : base(tregion)
        {
            MaxHealth = maxhealth;
            Health = maxhealth;
        }

        public double Health = 100;

        public double MaxHealth = 100;

        public virtual double GetHealth()
        {
            return Health;
        }

        public virtual double GetMaxHealth()
        {
            return MaxHealth;
        }

        public virtual void SetHealth(double health)
        {
            Health = Math.Min(health, MaxHealth);
            if (MaxHealth != 0 && Health <= 0)
            {
                Die();
            }
        }

        public virtual void Damage(double amount)
        {
            SetHealth(GetHealth() - amount);
        }

        public virtual void SetMaxHealth(double maxhealth)
        {
            MaxHealth = maxhealth;
            if (Health > MaxHealth)
            {
                SetHealth(MaxHealth);
            }
        }

        public abstract void Die();
    }
}
