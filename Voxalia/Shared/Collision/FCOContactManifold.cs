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
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z) || (vec.X == 0 && vec.Y == 0 && vec.Z == 0);
        }

        public static bool IsNaNOrInf(ref Vector3 vec)
        {
            return float.IsInfinity(vec.X) || float.IsNaN(vec.X)
                || float.IsInfinity(vec.Y) || float.IsNaN(vec.Y)
                || float.IsInfinity(vec.Z) || float.IsNaN(vec.Z);
        }
        
        public override void Update(float dt)
        {

            RigidTransform rt = convex.Entity == null ? convex.WorldTransform: new RigidTransform(convex.Entity.Position, convex.Entity.Orientation);
            if (IsNaNOrInf(ref rt.Position))
            {
                for (int i = contacts.Count - 1; i >= 0; i--)
                {
                    Remove(i);
                }
                return;
            }
            Vector3 sw = new Vector3(0, 0, 1f);
            if (convex.Entity != null)
            {
                sw = convex.Entity.LinearVelocity;
                float len = sw.Length();
                sw /= len;
            }
            RayHit rh;
            bool hit = mesh.ConvexCast(convex.Shape, ref rt, ref sw, 0.001f, MaterialSolidity.FULLSOLID, out rh);
            if (!hit || IsNaNOrInfOrZero(ref rh.Normal))
            {
                for (int i = contacts.Count - 1; i >= 0; i--)
                {
                    Remove(i);
                }
                return;
            }
            float pendef = 0.01f;
            Vector3 norm;
            RigidTransform rtx = new RigidTransform(Vector3.Zero, rt.Orientation);
            RigidTransform.Transform(ref rh.Normal, ref rtx, out norm);
            norm = -norm; // TODO: Why must we negate here?!
            for (int i = contacts.Count - 1; i >= 0; i--)
            {
                contacts[i].Normal = norm;
                contacts[i].Position = rh.Location;
                contacts[i].PenetrationDepth = pendef;
            }
            if (Contacts.Count == 0)
            {
                ContactData cd = new ContactData();
                cd.Normal = norm;
                cd.Position = rh.Location;
                cd.PenetrationDepth = pendef;
                cd.Id = contacts.Count;
                Add(ref cd);
            }
        }
        
        public override void CleanUp()
        {
            convex = null;
            mesh = null;
            base.CleanUp();
        }
    }
}
