﻿using System;
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
        /// <summary>
        /// Gets the contact constraint used by the pair handler.
        /// </summary>
        public override ContactManifoldConstraint ContactConstraint
        {
            get { return contactConstraint; }
        }
        /// <summary>
        /// Gets the contact manifold used by the pair handler.
        /// </summary>
        public override ContactManifold ContactManifold
        {
            get { return MeshManifold; }
        }

        FCOContactManifold contactManifold = new FCOContactManifold();

        protected FCOContactManifold MeshManifold { get { return contactManifold; } }

        public ConvexFCOPairHandler()
        {
            contactConstraint = new NonConvexContactManifoldConstraint(this);
        }

        bool noRecurse = false;

        ///<summary>
        /// Initializes the pair handler.
        ///</summary>
        ///<param name="entryA">First entry in the pair.</param>
        ///<param name="entryB">Second entry in the pair.</param>
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
                    throw new ArgumentException("Inappropriate types used to initialize pair.");
            }
            //Contact normal goes from A to B.
            BroadPhaseOverlap = new BEPUphysics.BroadPhaseSystems.BroadPhaseOverlap(convex, mesh);
            UpdateMaterialProperties(convex.Entity != null ? convex.Entity.Material : null, mesh.Material);
            base.Initialize(entryA, entryB);
            noRecurse = false;
        }


        ///<summary>
        /// Cleans up the pair handler.
        ///</summary>
        public override void CleanUp()
        {
            base.CleanUp();

            mesh = null;
            convex = null;
        }



        ///<summary>
        /// Updates the time of impact for the pair.
        ///</summary>
        ///<param name="requester">Collidable requesting the update.</param>
        ///<param name="dt">Timestep duration.</param>
        public override void UpdateTimeOfImpact(Collidable requester, float dt)
        {
            if (convex.Entity != null && convex.Entity.ActivityInformation.IsActive && convex.Entity.PositionUpdateMode == PositionUpdateMode.Continuous)
            {
                // TODO
            }

        }



        protected override void GetContactInformation(int index, out ContactInformation info)
        {
            ContactInformation ci = new ContactInformation();
            ci.Contact = contactManifold.ctcts[index];
            ci.Pair = this;
            ci.NormalImpulse = convex.Entity.Mass * 10f;
            ci.FrictionImpulse = convex.Entity.Mass;
            info = ci;
        }
    }
}
