using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using BEPUutilities;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SingleEntity;
using Voxalia.ServerGame.JointSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlaneEntity : VehicleEntity
    {
        public PlaneEntity(string pln, Region tregion)
            : base(pln, tregion)
        {
            SetMass(1000);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.HELICOPTER;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public bool ILeft = false;
        public bool IRight = false;

        public float ForwBack = 0;
        public float RightLeft = 0;

        public override void SpawnBody()
        {
            base.SpawnBody();
            Motion = new PlaneMotionConstraint(this);
            TheRegion.PhysicsWorld.Add(Motion);
            Wings = new JointFlyingDisc(this);
            TheRegion.AddJoint(Wings);
            // TODO: Wheels, like a car!
        }

        public float FastStrength
        {
            get
            {
                return GetMass() * 0.2f;
            }
        }

        public float RegularStrength
        {
            get
            {
                return GetMass() * 0.1f;
            }
        }

        public float SlowStrength
        {
            get
            {
                return GetMass() * 0.05f;
            }
        }

        public PlaneMotionConstraint Motion;
        
        // TODO: Plane-specific code?
        public JointFlyingDisc Wings;

        public override void Tick()
        {
            Motion.FlyFast = ILeft && !IRight;
            Motion.FlySlow = IRight && !ILeft;
            base.Tick();
        }

        public class PlaneMotionConstraint : SingleEntityConstraint // TODO: network!
        {
            PlaneEntity Plane;

            public bool FlyFast;
            public bool FlySlow;

            public PlaneMotionConstraint(PlaneEntity pln)
            {
                Plane = pln;
                Entity = pln.Body;
            }

            public override void ExclusiveUpdate()
            {
                if (Plane.DriverSeat.Sitter == null)
                {
                    return; // Don't fly when there's nobody driving this!
                }
                // Collect the plane's relative "forward" vector
                Vector3 forward = Quaternion.Transform(Vector3.UnitX, Entity.Orientation);
                // Engines!
                if (FlyFast)
                {
                    Vector3 force = forward * Plane.FastStrength;
                    entity.ApplyLinearImpulse(ref force);
                }
                else if (FlySlow)
                {
                    Vector3 force = forward * Plane.SlowStrength;
                    entity.ApplyLinearImpulse(ref force);
                }
                else // FlyNormal
                {
                    Vector3 force = forward * Plane.RegularStrength;
                    entity.ApplyLinearImpulse(ref force);
                }
                entity.ApplyImpulse(forward * 5 + entity.Position, new Vector3(0, 0, Plane.ForwBack) * entity.Mass * 0.005f);
                entity.ApplyImpulse(forward * 5 + entity.Position, new Vector3(0, Plane.RightLeft, 0) * entity.Mass * 0.005f);
                // Apply air drag
                Entity.ModifyLinearDamping(0.3f); // TODO: arbitrary constant
                Entity.ModifyAngularDamping(0.8f); // TODO: arbitrary constant
                // Ensure we're active if flying!
                Entity.ActivityInformation.Activate();
            }

            public override float SolveIteration()
            {
                return 0; // Do nothing
            }

            float Delta;

            public override void Update(float dt)
            {
                Delta = dt;
            }
        }

        public override void HandleInput(CharacterEntity character)
        {
            ILeft = character.ItemLeft;
            IRight = character.ItemRight;
            ForwBack = character.YMove;
            RightLeft = character.XMove;
        }
    }
}
