using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;
        public byte Dat;

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
            }
            else if (TheClient.FBOid == 2)
            {
                TheClient.s_colormultvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
            }
            Matrix4 mat = Matrix4.CreateTranslation(-Offset.ToOVector()) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            float spec = TheClient.Rendering.Specular;
            TheClient.Rendering.SetSpecular(0);
            vbo.Render(false);
            TheClient.Rendering.SetSpecular(spec);
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_fbo.Bind();
            }
            else if (TheClient.FBOid == 2)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.Shaders.ColorMultShader.Bind();
            }
        }
    }
}
