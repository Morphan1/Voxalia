//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities.Threading;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        public List<PlayerEntity> Players = new List<PlayerEntity>();

        /// <summary>
        /// All entities that exist on this server.
        /// TODO: Dictionary? Linked list? Experiment with speeds of different data types for common usages here!
        /// </summary>
        public List<Entity> Entities = new List<Entity>();

        /// <summary>
        /// All entities that exist on this server and must tick.
        /// </summary>
        public List<Entity> Tickers = new List<Entity>();

        public List<Entity> DespawnQuick = new List<Entity>();

        long jID = 0;

        public void AddJoint(InternalBaseJoint joint)
        {
            Joints.Add(joint);
            joint.One.Joints.Add(joint);
            joint.Two.Joints.Add(joint);
            joint.JID = jID++;
            joint.Enable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                pjoint.CurrentJoint = pjoint.GetBaseJoint();
                PhysicsWorld.Add(pjoint.CurrentJoint);
            }
            SendToAll(new AddJointPacketOut(joint));
        }

        public void DestroyJoint(InternalBaseJoint joint)
        {
            Joints.Remove(joint);
            joint.One.Joints.Remove(joint);
            joint.Two.Joints.Remove(joint);
            joint.Disable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                if (pjoint.CurrentJoint != null)
                {
                    try
                    {
                        PhysicsWorld.Remove(pjoint.CurrentJoint);
                    }
                    catch (Exception ex)
                    {
                        // We don't really care...
                        Utilities.CheckException(ex);
                    }
                }
            }
            SendToAll(new DestroyJointPacketOut(joint));
        }

        public void SpawnEntity(Entity e)
        {
            if (e.IsSpawned)
            {
                return;
            }
            Entities.Add(e);
            e.IsSpawned = true;
            if (e.EID < 1)
            {
                e.EID = TheServer.AdvanceCID();
            }
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            e.TheRegion = this;
            if (e is PhysicsEntity && !(e is PlayerEntity))
            {
                ((PhysicsEntity)e).ForceNetwork();
                ((PhysicsEntity)e).SpawnBody();
                ((PhysicsEntity)e).ForceNetwork();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Spawn();
            }
            if (e is PlayerEntity)
            {
                TheServer.Players.Add((PlayerEntity)e);
                Players.Add((PlayerEntity)e);
                for (int i = 0; i < TheServer.Networking.Strings.Strings.Count; i++)
                {
                    ((PlayerEntity)e).Network.SendPacket(new NetStringPacketOut(TheServer.Networking.Strings.Strings[i]));
                }
                ((PlayerEntity)e).SpawnBody();
                ((PlayerEntity)e).Network.SendPacket(new YourEIDPacketOut(e.EID));
                //((PlayerEntity)e).Network.SendPacket(new CVarSetPacketOut(TheServer.CVars.g_timescale, TheServer));
                ((PlayerEntity)e).SetAnimation("human/stand/idle01", 0);
                ((PlayerEntity)e).SetAnimation("human/stand/idle01", 1);
                ((PlayerEntity)e).SetAnimation("human/stand/idle01", 2);
            }
        }

        public void DespawnEntity(Entity e)
        {
            if (!e.IsSpawned)
            {
                return;
            }
            Entities.Remove(e);
            e.IsSpawned = false;
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                for (int i = 0; i < ((PhysicsEntity)e).Joints.Count; i++)
                {
                    DestroyJoint(((PhysicsEntity)e).Joints[i]);
                }
                ((PhysicsEntity)e).DestroyBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
            else if (e is PlayerEntity)
            {
                TheServer.Players.Remove((PlayerEntity)e);
                Players.Remove((PlayerEntity)e);
                ((PlayerEntity)e).Kick("Despawned!");
            }
            if (e.NetworkMe)
            {
                Location lpos = Location.NaN;
                if (e is PhysicsEntity)
                {
                    lpos = ((PhysicsEntity)e).lPos;
                }
                else if (e is PrimitiveEntity)
                {
                    lpos = ((PrimitiveEntity)e).lPos;
                }
                Location tpos = e.GetPosition();
                DespawnEntityPacketOut desppack = new DespawnEntityPacketOut(e.EID);
                foreach (PlayerEntity player in Players)
                {
                    if (player.ShouldSeePositionPreviously(lpos) || player.ShouldSeePosition(tpos))
                    {
                        player.Network.SendPacket(desppack);
                    }
                }
            }
        }

        public bool IsVisible(Location pos)
        {
            Vector3i cpos = ChunkLocFor(pos);
            foreach (PlayerEntity pe in Players)
            {
                if (pe.CanSeeChunk(cpos))
                {
                    return true;
                }
            }
            return false;
        }

        public void SendToVisible(Location pos, AbstractPacketOut packet)
        {
            Vector3i cpos = ChunkLocFor(pos);
            foreach (PlayerEntity pe in Players)
            {
                if (pe.CanSeeChunk(cpos))
                {
                    pe.Network.SendPacket(packet);
                }
            }
        }

        public List<PlayerEntity> GetPlayersInRadius(Location pos, double rad)
        {
            CheckThreadValidity();
            List<PlayerEntity> pes = new List<PlayerEntity>();
            foreach (PlayerEntity pe in Players)
            {
                if ((pe.GetPosition() - pos).LengthSquared() <= rad * rad)
                {
                    pes.Add(pe);
                }
            }
            return pes;
        }

        public List<Entity> GetEntitiesInRadius(Location pos, double rad)
        {
            List<Entity> es = new List<Entity>();
            // TODO: Efficiency!
            // TODO: Accuracy!
            double rx = rad * rad;
            foreach (Entity e in Entities)
            {
                if ((e.GetPosition().DistanceSquared(pos)) <= rx + e.GetScaleEstimate())
                {
                    es.Add(e);
                }
            }
            return es;
        }

        public PhysicsEntity ItemToEntity(ItemStack item)
        {
            if (item.Info is BlockItem)
            {
                return new BlockItemEntity(this, BlockInternal.FromItemDatum(item.Datum), Location.Zero);
            }
            if (item.Info is GlowstickItem)
            {
                return new GlowstickEntity(item.DrawColor, this);
            }
            if (item.Info is SmokegrenadeItem)
            {
                return new SmokeGrenadeEntity(item.DrawColor, this, item.GetAttributeI("big_smoke", 0) == 0 ? ParticleEffectNetType.SMOKE : ParticleEffectNetType.BIG_SMOKE)
                {
                    SmokeLeft = item.GetAttributeI("max_smoke", 300)
                };
            }
            if (item.Info is ExplosivegrenadeItem)
            {
                return new ExplosiveGrenadeEntity(this);
            }
            if (item.Info is PaintbombItem)
            {
                int paint = item.Datum;
                return new PaintBombEntity((byte)paint, this);
            }
            return new ItemEntity(item, this);
        }

        public Dictionary<EntityType, EntityConstructor> EntityConstructors = new Dictionary<EntityType, EntityConstructor>();

        public EntityConstructor ConstructorFor(EntityType etype)
        {
            EntityConstructor ec;
            if (EntityConstructors.TryGetValue(etype, out ec))
            {
                return ec;
            }
            return null;
        }

        public bool IgnoreEntities(BroadPhaseEntry entry)
        {
            return !(entry is EntityCollidable);
        }
        
        public void SpawnTree(string tree, Location opos, Chunk chunk)
        {
            // TODO: Efficiency!
            ModelEntity me = new ModelEntity("plants/trees/" + tree, this);
            Location pos = opos + new Location(0, 0, 1);
            /*RayCastResult rcr;
            bool h = SpecialCaseRayTrace(pos, -Location.UnitZ, 50, MaterialSolidity.FULLSOLID, IgnoreEntities, out rcr);
            me.SetPosition(h ? new Location(rcr.HitData.Location) : pos);*/
            Vector3 treealign = new Vector3(0, 0, 1);
            Vector3 norm = /*h ? rcr.HitData.Normal : */new Vector3(0, 0, 1);
            Quaternion orient;
            Quaternion.GetQuaternionBetweenNormalizedVectors(ref treealign, ref norm, out orient);
            orient *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (double)(Utilities.UtilRandom.NextDouble() * Math.PI * 2));
            me.SetOrientation(orient);
            me.SetPosition(pos);
            me.CanLOD = true;
            me.GenBlockShadow = true;
            Action res = () =>
            {
                SpawnEntity(me);
                me.SetPosition(pos - new Location(norm) - new Location(Quaternion.Transform(me.offset.ToBVector(), orient)));
                me.ForceNetwork();
            };
            if (chunk == null)
            {
                res.Invoke();
            }
            else
            {
                chunk.fixesToRun.Add(TheWorld.Schedule.GetSyncTask(res));
            }
        }
    }
}
