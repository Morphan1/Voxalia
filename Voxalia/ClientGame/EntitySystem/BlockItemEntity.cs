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
    class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;
        public byte Dat;
        public double soundmaxrate = 0.2;

        public BlockItemEntity(Region tregion, Material tmat, byte dat)
            : base(tregion, false, true)
        {
            Mat = tmat;
            Dat = dat;
            Shape = BlockShapeRegistry.BSD[dat].GetShape(out Offset);
            SetMass(5);
        }

        public Location Offset;

        public VBO vbo = null;

        public override void SpawnBody()
        {
            vbo = new VBO();
            List<BEPUutilities.Vector3> vecs = BlockShapeRegistry.BSD[Dat].GetVertices(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> norms = BlockShapeRegistry.BSD[Dat].GetNormals(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> tcoord = BlockShapeRegistry.BSD[Dat].GetTCoords(new BEPUutilities.Vector3(0, 0, 0), Mat, false, false, false, false, false, false);
            vbo.Vertices = new List<OpenTK.Vector3>();
            vbo.Normals = new List<OpenTK.Vector3>();
            vbo.TexCoords = new List<OpenTK.Vector3>();
            vbo.Indices = new List<uint>();
            for (int i = 0; i < vecs.Count; i++)
            {
                vbo.Vertices.Add(new OpenTK.Vector3(vecs[i].X, vecs[i].Y, vecs[i].Z));
                vbo.Normals.Add(new OpenTK.Vector3(norms[i].X, norms[i].Y, norms[i].Z));
                vbo.TexCoords.Add(new OpenTK.Vector3(tcoord[i].X, tcoord[i].Y, tcoord[i].Z));
                vbo.Indices.Add((uint)i);
            }
            vbo.GenerateVBO();
            base.SpawnBody();
            Body.CollisionInformation.Events.ContactCreated += Events_ContactCreated; // TODO: Perhaps better more direct event?
        }

        double soundlastplayed = 0;

        void Events_ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            if (other is FullChunkObject)
            {
                if (TheRegion.GlobalTickTimeLocal - soundlastplayed < soundmaxrate)
                {
                    return; // TODO: Better method of managing when to play sounds!
                }
                soundlastplayed = TheRegion.GlobalTickTimeLocal;
                Location block = new Location(contact.Position - contact.Normal * 0.01f);
                BlockInternal bi = TheRegion.GetBlockInternal(block);
                MaterialSound sound = ((Material)bi.BlockMaterial).Sound();
                if (sound != MaterialSound.NONE)
                {
                    // TODO: Adjust pitch/volume with impact velocity?
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound, 1, 1);
                }
                MaterialSound sound2 = Mat.Sound();
                if (sound2 != MaterialSound.NONE)
                {
                    // TODO: Adjust pitch/volume with impact velocity?
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound2, 1, 1);
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
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            { 
                TheClient.s_fbov.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 3)
            {
                TheClient.s_transponlyvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 4)
            {
                TheClient.s_shadowvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
            }
            Matrix4 mat = Matrix4.CreateTranslation(-ClientUtilities.Convert(Offset)) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            float spec = TheClient.Rendering.Specular;
            TheClient.Rendering.SetSpecular(0);
            vbo.Render(false);
            TheClient.Rendering.SetSpecular(spec);
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_fbo.Bind();
            }
            else if (TheClient.FBOid == 3)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_transponly.Bind();
            }
            else if (TheClient.FBOid == 4)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_shadow.Bind();
            }
        }
    }
}
