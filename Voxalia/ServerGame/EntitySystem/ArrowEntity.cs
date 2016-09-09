using System;
using Voxalia.Shared;
using BEPUutilities;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionRuleManagement;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class ArrowEntity: PrimitiveEntity
    {
        public ArrowEntity(Region tregion)
            : base(tregion)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
            Vector3 grav = TheRegion.PhysicsWorld.ForceUpdater.Gravity;
            Gravity = new Location(grav);
            Scale = new Location(0.05f, 0.05f, 0.05f);
        }
        
        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.PRIMITIVE;
        }

        public override byte[] GetNetData()
        {
            return GetPrimitiveNetData();
        }

        public override string GetModel()
        {
            return HasHat ? "projectiles/hat_arrow" : "projectiles/arrow";
        }

        public bool HasHat = false;

        public override void PotentialActivate()
        {
            Vector3 grav = TheRegion.PhysicsWorld.ForceUpdater.Gravity;
            Gravity = new Location(grav);
            base.PotentialActivate();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.ARROW;
        }

        public override BsonDocument GetSaveData()
        {
            // TODO: Maybe save as an item drop?
            // Does not save.
            return null;
        }
        
        public double Damage = 1;
        public double DamageTimesVelocity = 1;

        public PhysicsEntity StuckTo = null;

        public Matrix RelMat;

        public ModelEntity SolidHat = null;

        public override void Destroy()
        {
            SolidHat.RemoveMe();
            base.Destroy();
        }

        public override void Tick()
        {
            if (SolidHat != null)
            {
                SolidHat.SetPosition(GetPosition()); // TODO: offset this so it's centered on the bits of the arrow sticking out rather than inside the target?
                SolidHat.SetOrientation(GetOrientation());
            }
            if (StuckTo != null)
            {
                if (!StuckTo.IsSpawned)
                {
                    RemoveMe();
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
                    Angles *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 90f * (double)Utilities.PI180);
                }
            }
        }

        public bool Stuck = false;

        public void OnCollide(object sender, CollisionEventArgs args)
        {
            if (Stuck)
            {
                return;
            }
            double len = GetVelocity().Length();
            SetPosition(args.Info.Position + (GetVelocity() / len) * 0.05f);
            SetVelocity(Location.Zero);
            Gravity = Location.Zero;
            if (HasHat)
            {
                SolidHat = new ModelEntity("invisbox", TheRegion);
                SolidHat.SetMass(0);
                SolidHat.SetPosition(GetPosition());
                SolidHat.SetOrientation(GetOrientation());
                SolidHat.scale = new Location(0.6, 1.5, 0.6);
                SolidHat.Visible = false;
                SolidHat.CanSave = false;
                TheRegion.SpawnEntity(SolidHat);
            }
            if (args.Info.HitEnt != null)
            {
                PhysicsEntity pe = (PhysicsEntity)args.Info.HitEnt.Tag;
                if (pe is EntityDamageable)
                {
                    ((EntityDamageable)pe).Damage(Damage + DamageTimesVelocity * (double)len);
                }
                Vector3 loc = (args.Info.Position - pe.GetPosition()).ToBVector();
                Vector3 impulse = GetVelocity().ToBVector() * DamageTimesVelocity / 1000f;
                pe.Body.ApplyImpulse(ref loc, ref impulse);
                StuckTo = pe;
                if (HasHat)
                {
                    CollisionRules.AddRule(pe.Body, SolidHat.Body, CollisionRule.NoBroadPhase); // TODO: Broadcast this info! Perhaps abuse the joint system?
                }
            }
            TheRegion.SendToAll(new PrimitiveEntityUpdatePacketOut(this));
            if (args.Info.HitEnt != null)
            {
                PhysicsEntity pe = (PhysicsEntity)args.Info.HitEnt.Tag;
                JointForceWeld jfw = new JointForceWeld(pe, this);
                TheRegion.AddJoint(jfw);
            }
        }
    }
}
