using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using BEPUutilities;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

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

        public PhysicsEntity StuckTo = null;

        public Matrix RelMat;

        public override void Tick()
        {
            if (StuckTo != null)
            {
                if (!StuckTo.IsSpawned)
                {
                    TheServer.DespawnEntity(this);
                }
                else
                {
                    Matrix tmat = RelMat * StuckTo.Body.WorldTransform;
                    Location pos = Location.FromBVector(tmat.Translation);
                    Quaternion quat = Quaternion.CreateFromRotationMatrix(tmat);
                    if (pos != GetPosition() || quat != Angles)
                    {
                        Angles = quat;
                        SetPosition(pos);
                        TheServer.SendToAll(new PrimitiveEntityUpdatePacketOut(this)); // TODO: Simulate clientside
                    }
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
                    Angles *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 90f * (float)Utilities.PI180);
                }
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
                Vector3 loc = (args.Info.Position - pe.GetPosition()).ToBVector();
                Vector3 impulse = GetVelocity().ToBVector() * DamageTimesVelocity / 1000f;
                pe.Body.ApplyImpulse(ref loc, ref impulse);
                SetPosition(args.Info.Position + (GetVelocity() / len) * 0.05f);
                StuckTo = pe;
                SetVelocity(Location.Zero);
                Gravity = Location.Zero;
                Matrix worldTrans = pe.Body.WorldTransform;
                Matrix.Invert(ref worldTrans, out worldTrans);
                RelMat = (Matrix.CreateFromQuaternion(Angles)
                    * Matrix.CreateTranslation(GetPosition().ToBVector()))
                    * worldTrans;
            }
        }
    }
}
