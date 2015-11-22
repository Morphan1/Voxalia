using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
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
    /*
    public class MCCFCOPairHandler : StandardPairHandler
    {
        FullChunkObject mesh;

        MobileChunkCollidable mobile;

        private NonConvexContactManifoldConstraint contactConstraint;
        
        public override Collidable CollidableA
        {
            get { return mobile; }
        }

        public override Collidable CollidableB
        {
            get { return mesh; }
        }

        public override Entity EntityA
        {
            get { return mobile.Entity; }
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
        
        public MCCFCOPairHandler()
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
            mobile = entryB as MobileChunkCollidable;
            if (mesh == null || mobile == null)
            {
                mesh = entryB as FullChunkObject;
                mobile = entryA as MobileChunkCollidable;
                if (mesh == null || mobile == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
                }
            }
            broadPhaseOverlap = new BroadPhaseOverlap(mobile, mesh, broadPhaseOverlap.CollisionRule);
            UpdateMaterialProperties(mobile.Entity != null ? mobile.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }
        
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            mobile = null;
        }
        
        public override void UpdateTimeOfImpact(Collidable requester, float dt)
        {
            //Notice that we don't test for convex entity null explicitly.  The convex.IsActive property does that for us.
            if (mobile.IsActive && mobile.Entity.PositionUpdateMode == PositionUpdateMode.Continuous)
            {
                //Only perform the test if the minimum radii are small enough relative to the size of the velocity.
                Vector3 velocity = mobile.Entity.LinearVelocity * dt;
                float velocitySquared = velocity.LengthSquared();

                var minimumRadius = 1 * MotionSettings.CoreShapeScaling;
                timeOfImpact = 1;
                if (minimumRadius * minimumRadius < velocitySquared)
                {
                    for (int i = 0; i < contactManifold.ActivePairs.Count; i++)
                    {
                        var pair = contactManifold.ActivePairs.Values[i];
                        //In the contact manifold, the box collidable is always put into the second slot.
                        var boxCollidable = (ReusableGenericCollidable<ConvexShape>)pair.CollidableB;
                        RayHit rayHit;
                        var worldTransform = boxCollidable.WorldTransform;
                        if (GJKToolbox.CCDSphereCast(new Ray(mobile.WorldTransform.Position, velocity), minimumRadius, boxCollidable.Shape, ref worldTransform, timeOfImpact, out rayHit) &&
                            rayHit.T > Toolbox.BigEpsilon)
                        {
                            timeOfImpact = rayHit.T;
                        }
                    }
                }
            }
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
            if (mobile.Entity != null)
            {
                info.RelativeVelocity = Toolbox.GetVelocityOfPoint(info.Contact.Position, mobile.Entity.Position, mobile.Entity.LinearVelocity, mobile.Entity.AngularVelocity);
            }
            else
            {
                info.RelativeVelocity = new Vector3();
            }
            info.Pair = this;
        }
    }*/
}
