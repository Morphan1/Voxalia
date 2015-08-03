using System.Collections.Generic;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class CubeEntity : PhysicsEntity
    {
        public CubeEntity(Region tworld, Location halfsize)
            : base(tworld, true, true)
        {
            HalfSize = halfsize;
            Shape = new BoxShape((float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Recalculate();
            Color = new OpenTK.Graphics.Color4(0f, (float)Utilities.UtilRandom.NextDouble(), (float)Utilities.UtilRandom.NextDouble(), 1f);
        }

        public override void Tick()
        {
            base.Tick();
        }

        /// <summary>
        /// Half the size of the cuboid.
        /// </summary>
        public Location HalfSize;

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            Matrix4 mat = (Matrix4.CreateScale(HalfSize.ToOVector()) * GetOrientationMatrix() * Matrix4.CreateTranslation(GetPosition().ToOVector()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].Render(TheClient.RenderTextures);
            }
        }

        public override void SpawnBody()
        {
            Recalculate();
            base.SpawnBody();
        }

        public string[] Textures = new string[] { "top", "bottom", "xp", "xm", "yp", "ym" };
        
        public TextureCoordinates[] Coords = new TextureCoordinates[] { new TextureCoordinates(), new TextureCoordinates(),
            new TextureCoordinates(), new TextureCoordinates(), new TextureCoordinates(), new TextureCoordinates() };

        public List<VBO> VBOs = new List<VBO>();

        public void Recalculate()
        {
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].Destroy();
            }
            VBOs.Clear();
            GetVBOFor(TheClient.Textures.GetTexture(Textures[0])).AddSide(new Location(0, 0, 1), Coords[0]);
            GetVBOFor(TheClient.Textures.GetTexture(Textures[1])).AddSide(new Location(0, 0, -1), Coords[1]);
            GetVBOFor(TheClient.Textures.GetTexture(Textures[2])).AddSide(new Location(1, 0, 0), Coords[2]);
            GetVBOFor(TheClient.Textures.GetTexture(Textures[3])).AddSide(new Location(-1, 0, 0), Coords[3]);
            GetVBOFor(TheClient.Textures.GetTexture(Textures[4])).AddSide(new Location(0, 1, 0), Coords[4]);
            GetVBOFor(TheClient.Textures.GetTexture(Textures[5])).AddSide(new Location(0, -1, 0), Coords[5]);
            for (int i = 0; i < VBOs.Count; i++)
            {
                if (VBOs[i].Tex == TheClient.Textures.Clear)
                {
                    VBOs.RemoveAt(i);
                    i--;
                }
                else
                {
                    VBOs[i].GenerateVBO();
                }
            }
        }

        VBO GetVBOFor(Texture tex)
        {
            for (int i = 0; i < VBOs.Count; i++)
            {
                if (VBOs[i].Tex.Original_InternalID == tex.Original_InternalID)
                {
                    return VBOs[i];
                }
            }
            VBO vbo = new VBO();
            vbo.Tex = tex;
            vbo.Prepare();
            VBOs.Add(vbo);
            return vbo;
        }
    }
}
