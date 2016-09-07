using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.OtherSystems;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionTests;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.EntitySystem
{
    public class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;
        public byte Dat;
        public byte Paint;
        public BlockDamage Damage;
        public double soundmaxrate = 0.2;

        public BlockItemEntity(Region tregion, Material tmat, byte dat, byte tpaint, BlockDamage tdamage)
            : base(tregion, false, true)
        {
            Mat = tmat;
            Dat = dat;
            Paint = tpaint;
            Damage = tdamage;
            Shape = BlockShapeRegistry.BSD[dat].GetShape(Damage, out Offset);
            SetMass(5);
        }

        public Location Offset;

        public VBO vbo = null;

        public void GenVBO()
        {
            vbo = new VBO();
            List<BEPUutilities.Vector3> vecs = BlockShapeRegistry.BSD[Dat].GetVertices(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> norms = BlockShapeRegistry.BSD[Dat].GetNormals(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> tcoord = BlockShapeRegistry.BSD[Dat].GetTCoords(new BEPUutilities.Vector3(0, 0, 0), Mat, false, false, false, false, false, false);
            vbo.Vertices = new List<OpenTK.Vector3>();
            vbo.Normals = new List<OpenTK.Vector3>();
            vbo.TexCoords = new List<OpenTK.Vector3>();
            vbo.Indices = new List<uint>();
            vbo.Colors = new List<Vector4>();
            vbo.TCOLs = new List<Vector4>();
            vbo.Tangents = new List<Vector3>();
            vbo.THVs = new List<Vector4>();
            vbo.THWs = new List<Vector4>();
            System.Drawing.Color tcol = Voxalia.Shared.Colors.ForByte(Paint);
            for (int i = 0; i < vecs.Count; i++)
            {
                vbo.Vertices.Add(new OpenTK.Vector3(vecs[i].X, vecs[i].Y, vecs[i].Z));
                vbo.Normals.Add(new OpenTK.Vector3(norms[i].X, norms[i].Y, norms[i].Z));
                vbo.TexCoords.Add(new OpenTK.Vector3(tcoord[i].X, tcoord[i].Y, tcoord[i].Z));
                vbo.Indices.Add((uint)i);
                vbo.Colors.Add(new Vector4(1, 1, 1, 1));
                vbo.TCOLs.Add(TheClient.Rendering.AdaptColor(vbo.Vertices[i], tcol));
            }
            for (int i = 0; i < vecs.Count; i += 3)
            {
                int basis = i;
                OpenTK.Vector3 v1 = vbo.Vertices[basis];
                OpenTK.Vector3 dv1 = vbo.Vertices[basis + 1] - v1;
                OpenTK.Vector3 dv2 = vbo.Vertices[basis + 2] - v1;
                OpenTK.Vector3 t1 = vbo.TexCoords[basis];
                OpenTK.Vector3 dt1 = vbo.TexCoords[basis + 1] - t1;
                OpenTK.Vector3 dt2 = vbo.TexCoords[basis + 2] - t1;
                OpenTK.Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) * 1f / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                OpenTK.Vector3 normal = vbo.Normals[basis];
                tangent = (tangent - normal * OpenTK.Vector3.Dot(normal, tangent)).Normalized();
                for (int x = 0; x < 3; x++)
                {
                    vbo.Tangents.Add(tangent);
                    vbo.THVs.Add(new Vector4(0, 0, 0, 0));
                    vbo.THWs.Add(new Vector4(0, 0, 0, 0));
                }
            }
            vbo.GenerateVBO();
        }

        public override void SpawnBody()
        {
            GenVBO();
            base.SpawnBody();
            Body.CollisionInformation.Events.ContactCreated += Events_ContactCreated; // TODO: Perhaps better more direct event?
        }

        public double lastSoundTime;
        
        void Events_ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            if (TheRegion.GlobalTickTimeLocal - lastSoundTime < soundmaxrate)
            {
                return;
            }
            lastSoundTime = TheRegion.GlobalTickTimeLocal;
            if (other is FullChunkObject)
            {
                ContactInformation info;
                ((ConvexFCOPairHandler)pair).ContactInfo(/*contact.Id*/0, out info);
                float vellen = Math.Abs(info.RelativeVelocity.X) + Math.Abs(info.RelativeVelocity.Y) + Math.Abs(info.RelativeVelocity.Z);
                float mod = vellen / 5;
                if (mod > 2)
                {
                    mod = 2;
                }
                Location block = new Location(contact.Position - contact.Normal * 0.01f);
                BlockInternal bi = TheRegion.GetBlockInternal(block);
                MaterialSound sound = ((Material)bi.BlockMaterial).Sound();
                if (sound != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound, mod, 0.5f * mod);
                }
                MaterialSound sound2 = Mat.Sound();
                if (sound2 != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound2, mod, 0.5f * mod);
                }
            }
            else if (other is EntityCollidable)
            {
                BEPUphysics.Entities.Entity e = ((EntityCollidable)other).Entity;
                BEPUutilities.Vector3 velocity = BEPUutilities.Vector3.Zero;
                if (e != null)
                {
                    velocity = e.LinearVelocity;
                }
                BEPUutilities.Vector3 relvel = Body.LinearVelocity - velocity;
                float vellen = Math.Abs(relvel.X) + Math.Abs(relvel.Y) + Math.Abs(relvel.Z);
                float mod = vellen / 5;
                if (mod > 2)
                {
                    mod = 2;
                }
                MaterialSound sound = Mat.Sound();
                if (sound != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(new Location(contact.Position), sound, mod, 0.5f * mod);
                }
            }
        }

        public override void DestroyBody()
        {
            if (vbo != null)
            {
                vbo.Destroy();
            }
            base.DestroyBody();
        }
        
        public override void Render()
        {
            TheClient.SetVox();
            Matrix4 mat = Matrix4.CreateTranslation(-ClientUtilities.Convert(Offset)) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            vbo.Render(false);
        }
    }

    public class BlockItemEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] data)
        {
            Material mat = (Material)Utilities.BytesToUshort(Utilities.BytesPartial(data, PhysicsEntity.PhysicsNetworkDataLength, 2));
            byte dat = data[PhysicsEntity.PhysicsNetworkDataLength + 2];
            byte tpa = data[PhysicsEntity.PhysicsNetworkDataLength + 3];
            byte damage = data[PhysicsEntity.PhysicsNetworkDataLength + 4];
            BlockItemEntity bie = new BlockItemEntity(tregion, mat, dat, tpa, (BlockDamage)damage);
            bie.ApplyPhysicsNetworkData(data);
            return bie;
        }
    }
}
