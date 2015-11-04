using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class TextureBlock
    {
        public Client TheClient;

        public TextureEngine TEngine;

        public List<AnimatedTexture> Anims;

        public int TextureID = -1;

        public int HelpTextureID = -1;

        public int TWidth;

        public void Generate(Client tclient, ClientCVar cvars, TextureEngine eng)
        {
            TheClient = tclient;
            if (Anims != null)
            {
                for (int i = 0; i < Anims.Count; i++)
                {
                    Anims[i].Destroy();
                }
            }
            if (TextureID > -1)
            {
                GL.DeleteTexture(TextureID);
                GL.DeleteTexture(HelpTextureID);
            }
            Anims = new List<AnimatedTexture>();
            TEngine = eng;
            TextureID = GL.GenTexture();
            TWidth = cvars.r_blocktexturewidth.ValueI;
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, TWidth, TWidth, MaterialHelpers.MAX_TEXTURES);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)(cvars.r_blocktexturelinear.ValueB ? TextureMinFilter.Linear: TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)(cvars.r_blocktexturelinear.ValueB ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // Default Textures
            SetTexture((int)Material.AIR, "clear");
            SetTexture((int)Material.STONE, "blocks/solid/stone");
            SetTexture((int)Material.GRASS, "blocks/solid/grass_side");
            SetTexture((int)Material.DIRT, "blocks/solid/dirt");
            SetTexture((int)Material.WATER, "blocks/liquid/water");
            SetAnimated((int)Material.WATER, 0.25, new string[] { "blocks/liquid/water", "blocks/liquid/water_2", "blocks/liquid/water_3", "blocks/liquid/water_2" });
            SetTexture((int)Material.DEBUG, "blocks/solid/db_top");
            SetAnimated((int)Material.DEBUG, 1.0, new string[] { "blocks/solid/db_top", "blocks/solid/db_exc" });
            SetTexture((int)Material.LEAVES1, "blocks/transparent/leaves_basic1");
            SetTexture((int)Material.CONCRETE, "blocks/solid/concrete");
            SetTexture((int)Material.SLIPGOO, "blocks/liquid/slipgoo");
            SetTexture((int)Material.SNOW, "blocks/solid/snow");
            SetTexture((int)Material.SMOKE, "blocks/liquid/smoke");
            SetTexture((int)Material.LOG, "blocks/solid/wood");
            SetTexture((int)Material.TALLGRASS, "blocks/transparent/tallgrass");
            for (int i = (int)Material.NUM_DEFAULT; i < MaterialHelpers.MAX_TEXTURES - 7; i++)
            {
                SetTexture(i, "clear");
            }
            SetTexture(MaterialHelpers.MAX_TEXTURES - 7, "blocks/solid/wood_top");
            SetTexture(MaterialHelpers.MAX_TEXTURES - 6, "blocks/solid/db_ym");
            SetAnimated(MaterialHelpers.MAX_TEXTURES - 6, 1.0, new string[] { "blocks/solid/db_ym", "blocks/solid/db_exc" });
            SetTexture(MaterialHelpers.MAX_TEXTURES - 5, "blocks/solid/db_yp");
            SetAnimated(MaterialHelpers.MAX_TEXTURES - 5, 1.0, new string[] { "blocks/solid/db_yp", "blocks/solid/db_exc" });
            SetTexture(MaterialHelpers.MAX_TEXTURES - 4, "blocks/solid/db_xp");
            SetAnimated(MaterialHelpers.MAX_TEXTURES - 4, 1.0, new string[] { "blocks/solid/db_xp", "blocks/solid/db_exc" });
            SetTexture(MaterialHelpers.MAX_TEXTURES - 3, "blocks/solid/db_xm");
            SetAnimated(MaterialHelpers.MAX_TEXTURES - 3, 1.0, new string[] { "blocks/solid/db_xm", "blocks/solid/db_exc" });
            SetTexture(MaterialHelpers.MAX_TEXTURES - 2, "blocks/solid/db_bottom");
            SetAnimated(MaterialHelpers.MAX_TEXTURES - 2, 1.0, new string[] { "blocks/solid/db_bottom", "blocks/solid/db_exc" });
            SetTexture(MaterialHelpers.MAX_TEXTURES - 1, "blocks/solid/grass");
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            HelpTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, HelpTextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.R8, 2, 2, MaterialHelpers.MAX_TEXTURES);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            SetSettings((int)Material.WATER, 1, 0.1f);
            SetSettings((int)Material.DEBUG, 1, 0.1f);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        private void SetSettings(int id, float specular, float waviness)
        {
            float[] set = new float[] { specular, waviness, 0, 0 };
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, id, 2, 2, 1, PixelFormat.Red, PixelType.Float, set);
        }

        private void SetTexture(int ID, string texture)
        {
            TEngine.LoadTextureIntoArray(texture, ID, TWidth);
        }

        public void SetAnimated(int ID, double rate, string[] textures)
        {
            AnimatedTexture anim = new AnimatedTexture();
            anim.Block = this;
            anim.Level = ID;
            anim.Time = 0;
            anim.Rate = rate;
            anim.Textures = new int[textures.Length];
            anim.FBOs = new int[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                anim.Textures[i] = TEngine.GetTexture(textures[i], TWidth).Original_InternalID;
                anim.FBOs[i] = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, anim.FBOs[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, anim.Textures[i], 0);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            anim.Current = 0;
            anim.FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, anim.FBO);
            //GL.FramebufferTexture3D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DArray, TextureID, 0, ID);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureID, 0, ID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Anims.Add(anim);
        }

        public void Tick(double ttime)
        {
            for (int i = 0; i < Anims.Count; i++)
            {
                Anims[i].Tick(ttime);
            }
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }

    public class AnimatedTexture
    {
        public TextureBlock Block;

        public int Level;

        public double Rate;

        public double Time;

        public int FBO;

        public int[] FBOs;
        
        public void Tick(double ttime)
        {
            Time += ttime;
            if (Time > Rate)
            {
                AnimateNext();
                Time -= Rate;
            }
        }

        public int[] Textures;

        public int Current = 0;

        public void AnimateNext()
        {
            Current++;
            if (Current >= Textures.Length)
            {
                Current = 0;
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FBOs[Current]);
            GL.BlitFramebuffer(0, 0, Block.TWidth, Block.TWidth, 0, 0, Block.TWidth, Block.TWidth, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }

        public void Destroy()
        {
            GL.DeleteFramebuffer(FBO);
            for (int i = 0; i < Textures.Length; i++)
            {
                GL.DeleteTexture(Textures[i]);
            }
        }
    }
}
