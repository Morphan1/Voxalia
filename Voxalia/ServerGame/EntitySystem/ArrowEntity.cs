using System;
using Voxalia.Shared;
using BEPUutilities;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class ArrowEntity: PrimitiveEntity
    {
        public ArrowEntity(World tworld)
            : base(tworld)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
            Vector3 grav = TheWorld.PhysicsWorld.ForceUpdater.Gravity;
            Gravity = Location.FromBVector(grav);
            Scale = new Location(0.05f, 0.05f, 0.05f);
        }

        public float Damage = 1;
        public float DamageTimesVelocity = 1;

        public PhysicsEntity StuckTo = null;

        public Matrix RelMat;

        public override void Tick()
        {
            if (StuckTo != null)
            {
                if (!StuckTo.IsSpawned)
                {
                    TheWorld.DespawnEntity(this);
                }
            }
            else
            {
                base.Tick();
                Location vel = GetVelocity();
                if (vel.LengthSquared() > 0)
                {
                    Matrix lookatlh = Utilities.LookAtLH(Location.Zero, vel, Location.UnitZ);
                    lookatlh.Transpose();
                    Angles = Quaternion.CreateFromRotationMatrix(lookatlh);
                    Angles *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * (float)Utilities.PI180);
                }
            }
        }

        public void OnCollide(object sender, CollisionEventArgs args)
        {
            double len = GetVelocity().Length();
            if (len > 1)
            {
                if (args.Info.HitEnt != null)
                {
                    PhysicsEntity pe = (PhysicsEntity)args.Info.HitEnt.Tag;
                    if (pe is EntityDamageable)
                    {
                        ((EntityDamageable)pe).Damage(Damage + DamageTimesVelocity * (float)len);
                    }
                    Vector3 loc = (args.Info.Position - pe.GetPosition()).ToBVector();
                    Vector3 impulse = GetVelocity().ToBVector() * DamageTimesVelocity / 1000f;
                    pe.Body.ApplyImpulse(ref loc, ref impulse);
                    StuckTo = pe;
                }
                SetPosition(args.Info.Position + (GetVelocity() / len) * 0.05f);
                SetVelocity(Location.Zero);
                Gravity = Location.Zero;
                TheWorld.SendToAll(new PrimitiveEntityUpdatePacketOut(this));
                if (args.Info.HitEnt != null)
                {
                    PhysicsEntity pe = (PhysicsEntity)args.Info.HitEnt.Tag;
                    JointForceWeld jfw = new JointForceWeld(pe, this);
                    TheWorld.AddJoint(jfw);
                }
            }
        }
    }
}
