using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities.ResourceManagement;
using BEPUphysics.Settings;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using BEPUutilities.DataStructures;
using BEPUphysics.CollisionTests.Manifolds;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests;

namespace Voxalia.Shared.Collision
{
    public class FCOContactManifold : ContactManifold
    {
        protected ConvexCollidable convex;

        protected FullChunkObject mesh;

        public override void Initialize(Collidable newCollidableA, Collidable newCollidableB)
        {
            convex = newCollidableA as ConvexCollidable;
            mesh = newCollidableB as FullChunkObject;
            if (convex == null || mesh == null)
            {
                convex = newCollidableB as ConvexCollidable;
                mesh = newCollidableA as FullChunkObject;
                if (convex == null || mesh == null)
                {
                    throw new ArgumentException("Inappropriate types used to initialize contact manifold.");
                }
            }
        }

        public FCOContactManifold()
        {
            contacts = new RawList<Contact>();
            unusedContacts = new UnsafeResourcePool<Contact>(4);
            contactIndicesToRemove = new RawList<int>();
        }

        public RawList<Contact> ctcts
        {
            get
            {
                return contacts;
            }
        }

        public static bool IsNaNOrInfOrZero(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X) || vec.X == 0
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y) || vec.Y == 0
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z) || vec.Z == 0;
        }

        public override void Update(float dt)
        {
            RigidTransform rt = convex.WorldTransform;
            Vector3 sw = new Vector3(0, 0, -1f);
            RayHit rh;
            bool hit = mesh.ChunkShape.ConvexCast(convex.Shape, ref rt, ref sw, 0.001f, out rh);
            if (!hit || IsNaNOrInfOrZero(ref rh.Normal))
            {
                for (int i = contacts.Count - 1; i >= 0; i--)
                {
                    Remove(i);
                }
                return;
            }
            /*for (int i = contacts.Count - 1; i >= 0; i--)
            {
                contacts[i].Normal = rh.Normal;
                contacts[i].Position = rh.Location;
                contacts[i].PenetrationDepth = -0.1f;
            }*/
            if (Contacts.Count == 0)
            {
                ContactData cd = new ContactData();
                cd.Normal = rh.Normal;
                cd.Position = rh.Location;
                cd.PenetrationDepth = -0.1f;
                cd.Id = contacts.Count;
                Add(ref cd);
            }
        }
    }
}
