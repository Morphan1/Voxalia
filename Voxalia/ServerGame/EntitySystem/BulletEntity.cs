using System;
using Voxalia.Shared;
using BEPUutilities;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BulletEntity: PrimitiveEntity
    {
        public BulletEntity(World tworld)
            : base(tworld)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
            Gravity = Location.FromBVector(TheWorld.PhysicsWorld.ForceUpdater.Gravity);
        }

        public void OnCollide(object sender, CollisionEventArgs args)
        {
            if (args.Info.HitEnt != null)
            {
                PhysicsEntity physent = ((PhysicsEntity)args.Info.HitEnt.Tag);
                Vector3 loc = (GetPosition() - physent.GetPosition()).ToBVector();
                Vector3 impulse = GetVelocity().ToBVector() * Damage / 1000f;
                physent.Body.ApplyImpulse(ref loc, ref impulse);
                physent.Body.ActivityInformation.Activate();
                if (physent is EntityDamageable)
                {
                    ((EntityDamageable)physent).Damage(Damage);
                }
            }
            if (SplashSize > 0 && SplashDamage > 0)
            {
                // TODO: Apply Splash Damage
                // TODO: Apply Splash Impulses
            }
            TheWorld.DespawnEntity(this);
        }

        public override void Recalculate()
        {
            Scale = new Location(Size);
            base.Recalculate();
        }

        public float Size = 1;
        public float Damage = 1;
        public float SplashSize = 0;
        public float SplashDamage = 0;
    }
}
