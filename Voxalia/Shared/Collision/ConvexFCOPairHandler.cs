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
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.CollisionTests.CollisionAlgorithms.GJK;
using BEPUphysics.Constraints.Collision;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Entities;
using BEPUutilities;
using BEPUphysics;
using BEPUutilities.DataStructures;
using BEPUphysics.Materials;
using BEPUphysics.BroadPhaseSystems;
using BEPUutilities.ResourceManagement;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace Voxalia.Shared.Collision
{
    public class ConvexFCOPairHandler : StandardPairHandler
    {
        FullChunkObject mesh;

        ConvexCollidable convex;

        private NonConvexContactManifoldConstraint contactConstraint;
        
        public override Collidable CollidableA
        {
            get { return convex; }
        }

        public override Collidable CollidableB
        {
            get { return mesh; }
        }

        public override Entity EntityA
        {
            get { return convex.Entity; }
        }

        public override Entity EntityB
        {
            get { return null; }
        }

        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }

        public override ContactManifold ContactManifold
        {
            get { return contactManifold; }
        }

        FCOContactManifold contactManifold = new FCOContactManifold();
        
        public ConvexFCOPairHandler()
        {
            contactConstraint = new NonConvexContactManifoldConstraint(this);
        }

        bool noRecurse = false;
        
        public override void Initialize(BroadPhaseEntry entryA, BroadPhaseEntry entryB)
        {
            if (noRecurse)
            {
                return;
            }
            noRecurse = true;
            mesh = entryA as FullChunkObject;
            convex = entryB as ConvexCollidable;
            if (mesh == null || convex == null)
            {
                mesh = entryB as FullChunkObject;
                convex = entryA as ConvexCollidable;
                if (mesh == null || convex == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            broadPhaseOverlap = new BroadPhaseOverlap(convex, mesh, broadPhaseOverlap.CollisionRule);
            UpdateMaterialProperties(convex.Entity != null ? convex.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }
        
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            convex = null;
        }
        
        public override void UpdateTimeOfImpact(Collidable requester, double dt)
        {
            //Notice that we don't test for convex entity null explicitly.  The convex.IsActive property does that for us.
            if (convex.IsActive && convex.Entity.PositionUpdateMode == PositionUpdateMode.Continuous)
            {
                //Only perform the test if the minimum radii are small enough relative to the size of the velocity.
                Vector3 velocity = convex.Entity.LinearVelocity * dt;
                double velocitySquared = velocity.LengthSquared();
                double minimumRadius = convex.Shape.MinimumRadius * MotionSettings.CoreShapeScaling;
                timeOfImpact = 1;
                if (minimumRadius * minimumRadius < velocitySquared)
                {
                    for (int i = 0; i < contactManifold.ActivePairs.Count; i++)
                    {
                        GeneralConvexPairTester pair = contactManifold.ActivePairs.Values[i];
                        ReusableGenericCollidable<ConvexShape> boxCollidable = (ReusableGenericCollidable<ConvexShape>)pair.CollidableB;
                        RayHit rayHit;
                        RigidTransform worldTransform = boxCollidable.WorldTransform;
                        if (GJKToolbox.CCDSphereCast(new Ray(convex.WorldTransform.Position, velocity), minimumRadius, boxCollidable.Shape, ref worldTransform, timeOfImpact, out rayHit) &&
                            rayHit.T > Toolbox.BigEpsilon)
                        {
                            timeOfImpact = rayHit.T;
                        }
                    }
                }
            }
        }

        public void ContactInfo(int index, out ContactInformation info)
        {
            GetContactInformation(index, out info);
        }
        
        protected override void GetContactInformation(int index, out ContactInformation info)
        {
            info.Contact = contactManifold.Contacts[index];
            //Find the contact's normal and friction forces.
            info.FrictionImpulse = 0;
            info.NormalImpulse = 0;
            for (int i = 0; i < contactConstraint.ContactFrictionConstraints.Count; i++)
            {
                if (contactConstraint.ContactFrictionConstraints[i].PenetrationConstraint.Contact == info.Contact)
                {
                    info.FrictionImpulse = contactConstraint.ContactFrictionConstraints[i].TotalImpulse;
                    info.NormalImpulse = contactConstraint.ContactFrictionConstraints[i].PenetrationConstraint.NormalImpulse;
                    break;
                }
            }
            //Compute relative velocity
            if (convex.Entity != null)
            {
                info.RelativeVelocity = Toolbox.GetVelocityOfPoint(info.Contact.Position, convex.Entity.Position, convex.Entity.LinearVelocity, convex.Entity.AngularVelocity);
            }
            else
            {
                info.RelativeVelocity = new Vector3();
            }
            info.Pair = this;
        }
    }
}
