using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.EntitySystem;
using BEPUutilities.Threading;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    public class Region
    {
        /// <summary>
        /// The physics world in which all physics-related activity takes place.
        /// 
        /// </summary>
        public Space PhysicsWorld;

        public CollisionUtil Collision;

        public double Delta;

        public Location GravityNormal = new Location(0, 0, -1);

        public List<Entity> Entities = new List<Entity>();

        public List<Entity> Tickers = new List<Entity>();

        public List<Entity> ShadowCasters = new List<Entity>();
        
        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
            ParallelLooper pl = new ParallelLooper();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                pl.AddThread();
            }
            // Minimize penetration
            CollisionDetectionSettings.AllowedPenetration = 0.001f;
            PhysicsWorld = new Space(pl);
            // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, 0, -9.8f * 3f / 2f);
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
        }
#if NEW_CHUNKS
        public void AddChunk(FullChunkObject mesh)
        {
            if (mesh == null)
            {
                return;
            }
            PhysicsWorld.Add(mesh);
            return;
        }

        public void RemoveChunkQuiet(FullChunkObject mesh)
        {
            if (mesh == null)
            {
                return;
            }
            PhysicsWorld.Remove(mesh);
            return;
        }
#else
        public void AddChunk(StaticMesh mesh)
        {
            if (mesh == null)
            {
                return;
            }
            PhysicsWorld.Add(mesh);
            return;
        }

        public void RemoveChunkQuiet(StaticMesh mesh)
        {
            if (mesh == null)
            {
                return;
            }
            PhysicsWorld.Remove(mesh);
            return;
        }
#endif

        public bool SpecialCaseRayTrace(Location start, Location dir, float len, MaterialSolidity considerSolid, Func<BroadPhaseEntry, bool> filter, out RayCastResult rayHit)
        {
            Ray ray = new Ray(start.ToBVector(), dir.ToBVector());
            RayCastResult best = new RayCastResult(new RayHit() { T = len }, null);
            bool hA = false;
            if (considerSolid.HasFlag(MaterialSolidity.FULLSOLID))
            {
                RayCastResult rcr;
                if (PhysicsWorld.RayCast(ray, len, filter, out rcr))
                {
                    best = rcr;
                    hA = true;
                }
            }
            AABB box = new AABB();
            box.Min = start;
            box.Max = start;
            box.Include(start + dir * len);
            foreach (KeyValuePair<Location, Chunk> chunk in LoadedChunks)
            {
                if (chunk.Value == null || chunk.Value.FCO == null)
                {
                    continue;
                }
                if (!box.Intersects(new AABB() { Min = chunk.Value.WorldPosition * 30, Max = chunk.Value.WorldPosition * 30 + new Location(30, 30, 30) }))
                {
                    continue;
                }
                RayHit temp;
                if (chunk.Value.FCO.RayCast(ray, len, null, MaterialSolidity.ANY, out temp))
                {
                    hA = true;
                    temp.T *= len;
                    if (temp.T < best.HitData.T)
                    {
                        best.HitData = temp;
#if NEW_CHUNKS
                        best.HitObject = chunk.Value.FCO;
#else
                        best.HitObject = chunk.Value.worldObject;
#endif
                    }
                }
            }
            rayHit = best;
            return hA;
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            Delta = delta;
            GlobalTickTimeLocal += Delta;
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
            for (int i = 0; i < Tickers.Count; i++)
            {
                Tickers[i].Tick();
            }
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i].Enabled && Joints[i] is BaseFJoint)
                {
                    ((BaseFJoint)Joints[i]).Solve();
                }
            }
        }

        /// <summary>
        /// Spawns an entity in the world.
        /// </summary>
        /// <param name="e">The entity to spawn.</param>
        public void SpawnEntity(Entity e)
        {
            Entities.Add(e);
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Add(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).SpawnBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Spawn();
            }
        }

        public void Despawn(Entity e)
        {
            Entities.Remove(e);
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).DestroyBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
        }

        public Entity GetEntity(long EID)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].EID == EID)
                {
                    return Entities[i];
                }
            }
            return null;
        }

        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();

        public Client TheClient;

        public Location ChunkLocFor(Location pos)
        {
            pos.X = Math.Floor(pos.X / 30.0);
            pos.Y = Math.Floor(pos.Y / 30.0);
            pos.Z = Math.Floor(pos.Z / 30.0);
            return pos;
        }

        public Chunk LoadChunk(Location pos, int posMult)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                if (chunk.PosMultiplier != posMult)
                {
                    Chunk ch = chunk;
                    LoadedChunks.Remove(pos);
                    chunk = new Chunk(posMult);
                    chunk.OwningRegion = this;
                    chunk.adding = ch.adding;
                    chunk.worldObject = ch.worldObject;
                    chunk.rendering = ch.rendering;
                    chunk._VBO = ch._VBO;
                    chunk.WorldPosition = pos;
                    LoadedChunks.Add(pos, chunk);
                }
            }
            else
            {
                chunk = new Chunk(posMult);
                chunk.OwningRegion = this;
                chunk.WorldPosition = pos;
                LoadedChunks.Add(pos, chunk);
            }
            return chunk;
        }

        public Chunk GetChunk(Location pos)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                return chunk;
            }
            return null;
        }

        public Material GetBlockMaterial(Location pos)
        {
            return (Material)GetBlockInternal(pos).BlockMaterial;
        }

        public BlockInternal GetBlockInternal(Location pos)
        {
            Chunk ch = GetChunk(ChunkLocFor(pos));
            if (ch == null)
            {
                return BlockInternal.AIR;
            }
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30) / (float)ch.PosMultiplier);
            return ch.GetBlockAt(x, y, z);
        }

        public void SetBlockMaterial(Location pos, Material mat, byte dat = 0, bool regen = true)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos), 1);
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30) / (float)ch.PosMultiplier);
            ch.SetBlockAt(x, y, z, new BlockInternal((ushort)mat, dat, 1));
            if (regen)
            {
                UpdateChunk(ch);
                if (x == 0)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(-1, 0, 0)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }
                if (y == 0)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(0, -1, 0)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }
                if (z == 0)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(0, 0, -1)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }
                if (x == ch.CSize - 1)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(1, 0, 0)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }

                if (y == ch.CSize - 1)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(0, 1, 0)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }
                if (z == ch.CSize - 1)
                {
                    ch = GetChunk(ChunkLocFor(pos + new Location(0, 0, 1)));
                    if (ch != null)
                    {
                        UpdateChunk(ch);
                    }
                }
            }
        }

        public void UpdateChunk(Chunk ch)
        {
            TheClient.Schedule.StartASyncTask(() =>
            {
                ch.CalculateLighting();
                TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    ch.AddToWorld();
                    ch.CreateVBO();
                });
            });
        }

        public void Render()
        {
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0f);
            float spec = TheClient.Rendering.Specular;
            TheClient.Rendering.SetSpecular(0f);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref mat);
            if (TheClient.RenderTextures)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            foreach (Chunk chunk in LoadedChunks.Values)
            {
                if (TheClient.CFrust == null || TheClient.CFrust.ContainsBox(chunk.WorldPosition * 30, chunk.WorldPosition * 30 + new Location(30, 30, 30)))
                {
                    chunk.Render();
                }
            }
            TheClient.Rendering.SetSpecular(spec);
        }

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        public void AddJoint(InternalBaseJoint joint)
        {
            Joints.Add(joint);
            joint.One.Joints.Add(joint);
            joint.Two.Joints.Add(joint);
            joint.Enabled = true;
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                pjoint.CurrentJoint = pjoint.GetBaseJoint();
                PhysicsWorld.Add(pjoint.CurrentJoint);
            }
        }

        public void DestroyJoint(InternalBaseJoint joint)
        {
            Joints.Remove(joint);
            joint.One.Joints.Remove(joint);
            joint.Two.Joints.Remove(joint);
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                PhysicsWorld.Remove(pjoint.CurrentJoint);
            }
        }

        public double GlobalTickTimeLocal = 0;

        public void ForgetChunk(Location cpos)
        {
            Chunk ch;
            if (LoadedChunks.TryGetValue(cpos, out ch))
            {
                ch.Destroy();
                LoadedChunks.Remove(cpos);
            }
        }
    }
}
