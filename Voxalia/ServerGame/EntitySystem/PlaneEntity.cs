//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
using LiteDB;

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
            return EntityType.PLANE;
        }

        public override BsonDocument GetSaveData()
        {
            // TODO: Save properly!
            return null;
        }

        public bool ILeft = false;
        public bool IRight = false;

        public double FastOrSlow = 0f;

        public double ForwBack = 0;
        public double RightLeft = 0;

        public override void SpawnBody()
        {
            base.SpawnBody();
            Motion = new PlaneMotionConstraint(this);
            TheRegion.PhysicsWorld.Add(Motion);
            Wings = new JointFlyingDisc(this);
            TheRegion.AddJoint(Wings);
            HandleWheels();
        }

        // TODO: Customizable and networked speeds!
        public double FastStrength
        {
            get
            {
                return GetMass() * 12f;
            }
        }

        public double RegularStrength
        {
            get
            {
                return GetMass() * 3f;
            }
        }
        
        public PlaneMotionConstraint Motion;
        
        // TODO: Plane-specific code?
        public JointFlyingDisc Wings;

        public override void Tick()
        {
            // TODO: Raise/lower landing gear if player hits stance button!
            base.Tick();
        }

        public class PlaneMotionConstraint : SingleEntityConstraint
        {
            PlaneEntity Plane;
            
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
                // TODO: Special case for motion on land: only push forward if W key is pressed? Or maybe apply that rule in general?
                // Collect the plane's relative vectors
                Vector3 forward = Quaternion.Transform(Vector3.UnitY, Entity.Orientation);
                Vector3 side = Quaternion.Transform(Vector3.UnitX, Entity.Orientation);
                Vector3 up = Quaternion.Transform(Vector3.UnitZ, Entity.Orientation);
                // Engines!
                if (Plane.FastOrSlow >= 0.0)
                {
                    Vector3 force = forward * (Plane.RegularStrength + Plane.FastStrength) * Delta;
                    entity.ApplyLinearImpulse(ref force);
                }
                entity.ApplyImpulse(side * 5 + entity.Position, up * -Plane.RightLeft * entity.Mass * 1.5 * Delta);
                entity.ApplyImpulse(forward * 5 + entity.Position, side * ((Plane.IRight ? 1 : 0) + (Plane.ILeft ? -1 : 0)) * entity.Mass * 1.5 * Delta);
                if (Plane.ForwBack != 0.0)
                {
                    double dotforw = Vector3.Dot(entity.LinearVelocity, forward);
                    entity.ApplyImpulse(forward * 5 + entity.Position, up * Plane.ForwBack * entity.Mass * 0.05 * Delta * dotforw);
                    // Rotate the entity pre-emptively, and re-apply the movement velocity in this new direction!
                    /*double vellen = entity.LinearVelocity.Length();
                    Vector3 normvel = entity.LinearVelocity / vellen;
                    Vector3 norm_vel_transf = Quaternion.Transform(normvel, Quaternion.Inverse(entity.Orientation)); // Probably just 1,0,0 on whichever axis... can be simplified!
                    Vector3 inc = entity.AngularVelocity * Delta * 0.5;
                    Quaternion quat = new Quaternion(inc.X, inc.Y, inc.Z, 0);
                    quat = quat * entity.Orientation;
                    Quaternion orient = entity.Orientation;
                    Quaternion.Add(ref orient, ref quat, out orient);
                    orient.Normalize();
                    entity.Orientation = orient;
                    entity.LinearVelocity = Quaternion.Transform(norm_vel_transf, orient) * vellen;*/
                }
                // Apply air drag
                Entity.ModifyLinearDamping(Plane.FastOrSlow < 0.0 ? 0.6 : 0.1); // TODO: arbitrary constant
                Entity.ModifyAngularDamping(0.5); // TODO: arbitrary constant
                // Ensure we're active if flying!
                Entity.ActivityInformation.Activate();
            }

            public override double SolveIteration()
            {
                return 0; // Do nothing
            }

            double Delta;

            public override void Update(double dt)
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
            FastOrSlow = character.SprintOrWalk;
        }

        public override void Accepted(CharacterEntity character, Seat seat)
        {
            base.Accepted(character, seat);
            character.Desolidify();
        }

        public override void SeatKicked(CharacterEntity character, Seat seat)
        {
            base.SeatKicked(character, seat);
            character.Solidify();
        }
    }
}
