using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using BEPUutilities;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class ArrowEntity: PrimitiveEntity
    {
        public ArrowEntity(Server tserver)
            : base(tserver)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
            Vector3 grav = TheServer.PhysicsWorld.ForceUpdater.Gravity;
            Gravity = Location.FromBVector(grav);
            Scale = new Location(0.1f, 0.1f, 0.1f);
        }

        public float Damage = 1;
        public float DamageTimesVelocity = 1;

        public override void Tick()
        {
            base.Tick();
            Location vel = GetVelocity();
            if (vel.LengthSquared() > 0)
            {
                Location dir = Utilities.VectorToAngles(vel.Normalize());
                Angles = new Location(0, dir.Y, dir.X);
            }
        }

        public void OnCollide(object sender, CollisionEventArgs args)
        {
            double len = GetVelocity().Length();
            if (len > 1)
            {
                PhysicsEntity pe = (PhysicsEntity)args.Info.HitEnt.Tag;
                if (pe is EntityDamageable)
                {
                    ((EntityDamageable)pe).Damage(Damage + DamageTimesVelocity * (float)len);
                }
                Vector3 loc = (GetPosition() - pe.GetPosition()).ToBVector();
                Vector3 impulse = GetVelocity().ToBVector() * DamageTimesVelocity / 1000f;
                pe.Body.ApplyImpulse(ref loc, ref impulse);
                //SetPosition(Position + (GetVelocity() / len) * 0.1f);
                SetVelocity(Location.Zero);
            }
        }
    }
}
