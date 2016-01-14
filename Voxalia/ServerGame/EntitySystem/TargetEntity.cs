using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.EntitySystem
{
    class TargetEntity: ModelEntity, EntityDamageable
    {
        public TargetEntity(Region tregion) :
            base ("cube", tregion)
        {
            SetMass(10);
        }

        public float Health = 100;

        public float MaxHealth = 100;

        public float GetHealth()
        {
            return Health;
        }

        public double NextBoing = 0;

        public override void Tick()
        {
            base.Tick();
            NextBoing -= TheRegion.Delta;
            if (NextBoing <= 0)
            {
                NextBoing = Utilities.UtilRandom.NextDouble() * 2;
                ApplyForce(new Location(Utilities.UtilRandom.NextDouble() * GetMass(), Utilities.UtilRandom.NextDouble() * GetMass(), GetMass() * Utilities.UtilRandom.NextDouble() * 10));
            }
        }

        public float GetMaxHealth()
        {
            return MaxHealth;
        }

        public void SetHealth(float health)
        {
            Health = health;
            if (Health <= 0)
            {
                Kill();
            }
        }

        public void SetMaxHealth(float health)
        {
            MaxHealth = health;
        }

        public void Damage(float amount)
        {
            SetHealth(GetHealth() - amount);
        }

        public void Kill()
        {
            if (Removed)
            {
                return;
            }
            TheRegion.Explode(GetPosition(), 5);
            RemoveMe();
        }
    }
}
