using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using BEPUutilities;

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;
        public byte Dat;

        public BlockItemEntity(World tworld, Material tmat, byte dat)
            : base(tworld, false, true)
        {
            Mat = tmat;
            Dat = dat;
            if (dat == 0)
            {
                Shape = new BoxShape(1, 1, 1);
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = BlockShapeRegistry.BSD[Dat].GetVertices(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
                int[] ints = new int[vecs.Count];
                for (int i = 0; i < vecs.Count; i++)
                {
                    ints[i] = i;
                }
                BEPUutilities.Vector3 offs;
                Shape = new MobileMeshShape(vecs.ToArray(), ints, new AffineTransform(new BEPUutilities.Vector3(0.95f, 0.95f, 0.95f),
                    BEPUutilities.Quaternion.Identity, BEPUutilities.Vector3.Zero), MobileMeshSolidity.DoubleSided, out offs);
                Offset = Location.FromBVector(offs);
            }
            SetMass(5);
        }

        public Location Offset;

        public VBO vbo = null;

        public override void SpawnBody()
        {
            vbo = new VBO();
            if (Dat == 0)
            {
                vbo.Prepare();
                vbo.AddSide(new Location(0, 0, 1), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.TOP));
                vbo.AddSide(new Location(0, 0, -1), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.BOTTOM));
                vbo.AddSide(new Location(0, 1, 0), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.YP));
                vbo.AddSide(new Location(0, -1, 0), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.YM));
                vbo.AddSide(new Location(1, 0, 0), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.XP));
                vbo.AddSide(new Location(-1, 0, 0), new TextureCoordinates(), true, Mat.TextureID(MaterialSide.XM));
            }
            else
            {
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
            if (TheClient.FBO)
            { 
                 // TODO: Remove this block
                TheClient.s_fbov.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
            }
            Matrix4 mat = Matrix4.CreateTranslation(-Offset.ToOVector()) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            vbo.Render(false);
            if (TheClient.FBO)
            {
                // TODO: Remove this block
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_fbo.Bind();
            }
        }
    }
}
