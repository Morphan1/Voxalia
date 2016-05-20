using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.OtherSystems;
using Voxalia.ServerGame.NetworkSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class ModelEntity: PhysicsEntity
    {
        public override EntityType GetEntityType()
        {
            return EntityType.MODEL;
        }

        public bool CanLOD = false;

        public override AbstractPacketOut GetLODSpawnPacket()
        {
            if (CanLOD)
            {
                return new LODModelPacketOut(this);
            }
            return null;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] modelname = Utilities.encoding.GetBytes(model);
            byte[] bbytes = GetPhysicsBytes();
            byte[] res = new byte[bbytes.Length + 1 + 4 + modelname.Length + 1];
            bbytes.CopyTo(res, 0);
            res[bbytes.Length] = (byte)mode;
            Utilities.IntToBytes(modelname.Length).CopyTo(res, bbytes.Length + 1);
            modelname.CopyTo(res, bbytes.Length + 1 + 4);
            res[bbytes.Length + 1 + 4 + modelname.Length] = (byte)(CanLOD ? 1 : 0);
            return res;
        }

        public string model;

        public Location scale = Location.One;

        public ModelEntity(string mod, Region tregion)
            : base(tregion)
        {
            model = mod;
        }
        
        public ModelCollisionMode mode = ModelCollisionMode.AABB;
        
        public Location offset;

        int modelVerts = 10;

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + modelVerts * 12;
        }

        public override void SpawnBody()
        {
            Model smod = TheServer.Models.GetModel(model);
            if (smod == null) // TODO: smod should return a cube when all else fails?
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            Model3D smodel = smod.Original;
            if (smodel == null) // TODO: smodel should return a cube when all else fails?
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheServer.Models.handler.MeshToBepu(smodel, out modelVerts); // TODO: Scale!
            }
            if (mode == ModelCollisionMode.CONVEXHULL)
            {
                Shape = TheServer.Models.handler.MeshToBepuConvex(smodel, out modelVerts); // TODO: Scale!
            }
            else if (mode == ModelCollisionMode.AABB)
            {
                List<BEPUutilities.Vector3> vecs = TheServer.Models.handler.GetCollisionVertices(smodel);
                Location zero = new Location(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(new Location(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                offset = -center;
                Shape = new BoxShape((float)size.X * (float)scale.X, (float)size.Y * (float)scale.Y, (float)size.Z * (float)scale.Z);
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheServer.Models.handler.GetCollisionVertices(smodel);
                double distSq = 0;
                for (int v = 1; v < vecs.Count; v++)
                {
                    if (vecs[v].LengthSquared() > distSq)
                    {
                        distSq = vecs[v].LengthSquared();
                    }
                }
                double size = Math.Sqrt(distSq);
                offset = Location.Zero;
                Shape = new SphereShape((float)size * (float)scale.X);
            }
            base.SpawnBody();
            if (mode == ModelCollisionMode.PRECISE)
            {
                offset = InternalOffset;
            }
        }
    }

    public class ModelEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4 + 1;
            byte mode = input[plen];
            int namelen = Utilities.BytesToInt(Utilities.BytesPartial(input, plen + 1, 4));
            string name = Utilities.encoding.GetString(input, plen + 1 + 4, namelen);
            ModelEntity ent = new ModelEntity(name, tregion);
            ent.mode = (ModelCollisionMode)mode;
            ent.CanLOD = input[plen + 1 + 4 + namelen] == 1;
            ent.ApplyBytes(input);
            return ent;
        }
    }
}
