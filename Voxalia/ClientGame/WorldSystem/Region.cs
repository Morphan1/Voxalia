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
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using System.Diagnostics;
using Priority_Queue;
using FreneticScript;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Region
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

        public AABB[] Highlights = new AABB[0];

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
            CollisionDetectionSettings.AllowedPenetration = 0.01f;
            PhysicsWorld = new Space(pl);
            PhysicsWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, 0, -9.8f * 3f / 2f);
            PhysicsWorld.DuringForcesUpdateables.Add(new LiquidVolume(this));
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
        }
        public void AddChunk(FullChunkObject mesh)
        {
            PhysicsWorld.Add(mesh);
        }

        public void RemoveChunkQuiet(FullChunkObject mesh)
        {
            PhysicsWorld.Remove(mesh);
        }

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
            foreach (KeyValuePair<Vector3i, Chunk> chunk in LoadedChunks)
            {
                if (chunk.Value == null || chunk.Value.FCO == null)
                {
                    continue;
                }
                if (!box.Intersects(new AABB() { Min = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, Max = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE) }))
                {
                    continue;
                }
                RayHit temp;
                if (chunk.Value.FCO.RayCast(ray, len, null, considerSolid, out temp))
                {
                    hA = true;
                    //temp.T *= len;
                    if (temp.T < best.HitData.T)
                    {
                        best.HitData = temp;
                        best.HitObject = chunk.Value.FCO;
                    }
                }
            }
            rayHit = best;
            return hA;
        }

        public bool SpecialCaseConvexTrace(ConvexShape shape, Location start, Location dir, float len, MaterialSolidity considerSolid, Func<BroadPhaseEntry, bool> filter, out RayCastResult rayHit)
        {
            RigidTransform rt = new RigidTransform(start.ToBVector(), BEPUutilities.Quaternion.Identity);
            BEPUutilities.Vector3 sweep = (dir * len).ToBVector();
            RayCastResult best = new RayCastResult(new RayHit() { T = len }, null);
            bool hA = false;
            if (considerSolid.HasFlag(MaterialSolidity.FULLSOLID))
            {
                RayCastResult rcr;
                if (PhysicsWorld.ConvexCast(shape, ref rt, ref sweep, filter, out rcr))
                {
                    best = rcr;
                    hA = true;
                }
            }
            sweep = dir.ToBVector();
            AABB box = new AABB();
            box.Min = start;
            box.Max = start;
            box.Include(start + dir * len);
            foreach (KeyValuePair<Vector3i, Chunk> chunk in LoadedChunks)
            {
                if (chunk.Value == null || chunk.Value.FCO == null)
                {
                    continue;
                }
                if (!box.Intersects(new AABB() { Min = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, Max = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE) }))
                {
                    continue;
                }
                RayHit temp;
                if (chunk.Value.FCO.ConvexCast(shape, ref rt, ref sweep, len, considerSolid, out temp))
                {
                    hA = true;
                    //temp.T *= len;
                    if (temp.T < best.HitData.T)
                    {
                        best.HitData = temp;
                        best.HitObject = chunk.Value.FCO;
                    }
                }
            }
            rayHit = best;
            return hA;
        }

        public double PhysTime;

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            Delta = delta;
            if (Delta <= 0)
            {
                return;
            }
            GlobalTickTimeLocal += Delta;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
            sw.Stop();
            PhysTime = (double)sw.ElapsedMilliseconds / 1000f;
            for (int i = 0; i < Tickers.Count; i++)
            {
                Tickers[i].Tick();
            }
            SolveJoints();
            TickClouds();
            CheckForRenderNeed();
        }

        public void SolveJoints()
        {
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
                for (int i = 0; i < ((PhysicsEntity)e).Joints.Count; i++)
                {
                    DestroyJoint(((PhysicsEntity)e).Joints[i]);
                }
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
        }

        public InternalBaseJoint GetJoint(long JID)
        {
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i].JID == JID)
                {
                    return Joints[i];
                }
            }
            return null;
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

        public Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

        public Client TheClient;

        public Vector3i ChunkLocFor(Location pos)
        {
            Vector3i temp;
            temp.X = (int)Math.Floor(pos.X / Chunk.CHUNK_SIZE);
            temp.Y = (int)Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
            temp.Z = (int)Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
            return temp;
        }

        public Chunk LoadChunk(Vector3i pos, int posMult)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                // TODO: ?!?!?!?
                if (chunk.PosMultiplier != posMult)
                {
                    Chunk ch = chunk;
                    LoadedChunks.Remove(pos);
                    chunk = new Chunk(posMult);
                    chunk.OwningRegion = this;
                    chunk.adding = ch.adding;
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

        public Chunk GetChunk(Vector3i pos)
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
                return new BlockInternal((ushort)Material.AIR, 0, 0, 128);
            }
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            return ch.GetBlockAt(x, y, z);
        }

        public void Regen(Location pos, Chunk ch, int x = 1, int y = 1, int z = 1)
        {
            UpdateChunk(ch);
            if (x == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(-1, 0, 0)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (y == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(0, -1, 0)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (z == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(0, 0, -1)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (x == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(1, 0, 0)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (y == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(0, 1, 0)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (z == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(0, 0, 1)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            // TODO: Cascade downward for lighting updates?
        }

        public void SetBlockMaterial(Location pos, ushort mat, byte dat = 0, byte paint = 0, bool regen = true)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos), 1);
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            ch.SetBlockAt(x, y, z, new BlockInternal(mat, dat, paint, 0));
            if (regen)
            {
                Regen(pos, ch, x, y, z);
            }
        }

        public void UpdateChunk(Chunk ch)
        {
            /*TheClient.Schedule.StartASyncTask(() =>
            {
                ch.CalculateLighting();
            });*/
            TheClient.Schedule.ScheduleSyncTask(() =>
            {
                ch.AddToWorld();
                ch.CreateVBO();
            });
        }

        public void RenderEffects()
        {
            GL.LineWidth(5);
            TheClient.Rendering.SetColor(Color4.White);
            for (int i = 0; i < Highlights.Length; i++)
            {
                TheClient.Rendering.RenderLineBox(Highlights[i].Min, Highlights[i].Max);
            }
            GL.LineWidth(1);
        }

        public void Render()
        {
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0f);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref mat);
            if (TheClient.RenderTextures)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            /*foreach (Chunk chunk in LoadedChunks.Values)
            {
                if (TheClient.CFrust == null || TheClient.CFrust.ContainsBox(chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE)))
                {
                    chunk.Render();
                }
            }*/
            if (TheClient.FBOid == FBOID.MAIN || TheClient.FBOid == FBOID.NONE || TheClient.FBOid == FBOID.FORWARD_SOLID)
            {
                chToRender.Clear();
                ChunkMarchAndDraw();
            }
            else
            {
                foreach (Chunk ch in chToRender)
                {
                    ch.Render();
                }
            }
            if (TheClient.RenderTextures)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        public List<Chunk> chToRender = new List<Chunk>();

        static Vector3i[] MoveDirs = new Vector3i[] { new Vector3i(-1, 0, 0), new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, -1), new Vector3i(0, 0, 1) };

        public int MaxRenderDistanceChunks = 10;

        public void ChunkMarchAndDraw()
        {
            Vector3i start = ChunkLocFor(TheClient.CameraPos);
            HashSet<Vector3i> seen = new HashSet<Vector3i>();
            Queue<Vector3i> toSee = new Queue<Vector3i>();
            toSee.Enqueue(start);
            while (toSee.Count > 0)
            {
                Vector3i cur = toSee.Dequeue();
                if ((Math.Abs(cur.X - start.X) > MaxRenderDistanceChunks)
                    || (Math.Abs(cur.Y - start.Y) > MaxRenderDistanceChunks)
                    || (Math.Abs(cur.Z - start.Z) > MaxRenderDistanceChunks))
                {
                    continue;
                }
                seen.Add(cur);
                Chunk chcur = GetChunk(cur);
                if (chcur != null)
                {
                    chcur.Render();
                    chToRender.Add(chcur);
                }
                for (int i = 0; i < MoveDirs.Length; i++)
                {
                    Vector3i t = cur + MoveDirs[i];
                    if (!seen.Contains(t) && !toSee.Contains(t))
                    {
                        //toSee.Enqueue(t);
                        for (int j = 0; j < MoveDirs.Length; j++)
                        {
                            if (BEPUutilities.Vector3.Dot(MoveDirs[j].ToVector3(), (TheClient.CameraTarget - TheClient.CameraPos).ToBVector()) < -0.8f) // TODO: Wut?
                            {
                                continue;
                            }
                            Vector3i nt = cur + MoveDirs[j];
                            if (!seen.Contains(nt) && !toSee.Contains(nt))
                            {
                                bool val = false;
                                Chunk ch = GetChunk(t);
                                if (ch == null)
                                {
                                    val = true;
                                }
                                // TODO: Oh, come on!
                                else if (MoveDirs[i].X == -1)
                                {
                                    if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_XM];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XP];
                                    }
                                }
                                else if (MoveDirs[i].X == 1)
                                {
                                    if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_XM];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XM];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XM];
                                    }
                                }
                                else if (MoveDirs[i].Y == -1)
                                {
                                    if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.YP_YM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Y == 1)
                                {
                                    if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.YP_YM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Z == -1)
                                {
                                    if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_ZM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XM];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XP];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Z == 1)
                                {
                                    if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_ZM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XM];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XP];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                }
                                if (val)
                                {
                                    BEPUutilities.Vector3 min = nt.ToVector3() * Chunk.CHUNK_SIZE;
                                    if (TheClient.CFrust == null || TheClient.CFrust.ContainsBox(min, min + new BEPUutilities.Vector3(Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE)))
                                    {
                                        toSee.Enqueue(nt);
                                    }
                                    else
                                    {
                                        seen.Add(nt);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        public void AddJoint(InternalBaseJoint joint)
        {
            Joints.Add(joint);
            joint.One.Joints.Add(joint);
            joint.Two.Joints.Add(joint);
            joint.Enable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                pjoint.CurrentJoint = pjoint.GetBaseJoint();
                PhysicsWorld.Add(pjoint.CurrentJoint);
            }
        }

        public void DestroyJoint(InternalBaseJoint joint)
        {
            if (!Joints.Remove(joint))
            {
                SysConsole.Output(OutputType.WARNING, "Destroyed non-existent joint?!");
            }
            joint.One.Joints.Remove(joint);
            joint.Two.Joints.Remove(joint);
            joint.Disable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                PhysicsWorld.Remove(pjoint.CurrentJoint);
            }
        }

        public double GlobalTickTimeLocal = 0;

        public void ForgetChunk(Vector3i cpos)
        {
            Chunk ch;
            if (LoadedChunks.TryGetValue(cpos, out ch))
            {
                ch.Destroy();
                LoadedChunks.Remove(cpos);
            }
        }

        public bool InWater(Location min, Location max)
        {
            // TODO: Efficiency!
            min = min.GetBlockLocation();
            max = max.GetUpperBlockBorder();
            for (int x = (int)min.X; x < max.X; x++)
            {
                for (int y = (int)min.Y; y < max.Y; y++)
                {
                    for (int z = (int)min.Z; z < max.Z; z++)
                    {
                        if (GetBlockMaterial(min + new Location(x, y, z)).GetSolidity() == MaterialSolidity.LIQUID)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public Location GetSkyLight(Location pos, Location norm)
        {
            if (norm.Z < -0.99)
            {
                return Location.Zero;
            }
            pos.Z = pos.Z + 1;
            int XP = (int)Math.Floor(pos.X / Chunk.CHUNK_SIZE);
            int YP = (int)Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
            int ZP = (int)Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
            int x = (int)(Math.Floor(pos.X) - (XP * Chunk.CHUNK_SIZE));
            int y = (int)(Math.Floor(pos.Y) - (YP * Chunk.CHUNK_SIZE));
            int z = (int)(Math.Floor(pos.Z) - (ZP * Chunk.CHUNK_SIZE));
            float light = 1f;
            while (true)
            {
                Chunk ch = GetChunk(new Vector3i(XP, YP, ZP));
                if (ch == null)
                {
                    break;
                }
                while (z < Chunk.CHUNK_SIZE)
                {
                    BlockInternal bi = ch.GetBlockAtLOD((int)x, (int)y, (int)z);
                    if (bi.IsOpaque())
                    {
                        Material mat = (Material)bi.BlockMaterial;
                        float lrange = mat.GetLightEmitRange();
                        if (lrange > 0)
                        {
                            int biz = z + ZP * Chunk.CHUNK_SIZE;
                            int dist = biz - (int)pos.Z;
                            if (dist <= 0)
                            {
                                return mat.GetLightEmit();
                            }
                            if (dist >= lrange)
                            {
                                return Location.Zero;
                            }
                            return mat.GetLightEmit() * (1f - dist / lrange) * SkyLightMod;
                        }
                        return Location.Zero;
                    }
                    light -= ((Material)bi.BlockMaterial).GetLightDamage();
                    z++;
                }
                ZP++;
                z = 0;
            }
            return Math.Max(norm.Dot(SunLightPathNegative), 0.5) * new Location(light) * SkyLightMod;
        }

        static Location SunLightPathNegative = new Location(0, 0, 1);

        const float SkyLightMod = 0.75f;

        public Location GetAmbient(Location pos)
        {
            return TheClient.BaseAmbient;
        }

        public Location Regularize(Location col)
        {
            if (col.X < 1.0 && col.Y < 1.0 && col.Z < 1.0)
            {
                return col;
            }
            return col / Math.Max(col.X, Math.Max(col.Y, col.Z));
        }

        public Location GetLightAmount(Location pos, Location norm)
        {
            Location amb = GetAmbient(pos);
            Location sky = GetSkyLight(pos, norm);
            return Regularize(amb + sky);
        }
        
        public SimplePriorityQueue<Vector3i> NeedsRendering = new SimplePriorityQueue<Vector3i>();

        public HashSet<Vector3i> RenderingNow = new HashSet<Vector3i>();

        public void CheckForRenderNeed()
        {
            lock (RenderingNow)
            {
                while (NeedsRendering.Count > 0 && RenderingNow.Count < TheClient.CVars.r_chunksatonce.ValueI)
                {
                    Vector3i temp = NeedsRendering.Dequeue();
                    Chunk ch = GetChunk(temp);
                    if (ch != null)
                    {
                        ch.MakeVBONow();
                        RenderingNow.Add(temp);
                    }
                }
            }
        }

        public void DoneRendering(Chunk ch)
        {
            lock (RenderingNow)
            {
                RenderingNow.Remove(ch.WorldPosition);
            }
        }

        public void NeedToRender(Chunk ch)
        {
            lock (RenderingNow)
            {
                if (!NeedsRendering.Contains(ch.WorldPosition))
                {
                    NeedsRendering.Enqueue(ch.WorldPosition, (ch.WorldPosition.ToLocation() - TheClient.CameraPos).LengthSquared());
                }
            }
        }
    }
}
