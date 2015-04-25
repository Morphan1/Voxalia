using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using BEPUutilities;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class BulletEntity: PrimitiveEntity
    {
        public BulletEntity(Server tserver)
            : base(tserver)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
        }

        public void OnCollide(object sender, CollisionEventArgs args)
        {
            Vector3 loc = (GetPosition() - ((PhysicsEntity)args.Info.HitEnt.Tag).GetPosition()).ToBVector();
            Vector3 impulse = GetVelocity().ToBVector() * Damage / 1000f;
            ((PhysicsEntity)args.Info.HitEnt.Tag).Body.ApplyImpulse(ref loc, ref impulse);
            TheServer.DespawnEntity(this);
            // TODO: Apply Damage
            // TODO: Apply Splash Damage
            // TODO: Apply Splash Impulses
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
