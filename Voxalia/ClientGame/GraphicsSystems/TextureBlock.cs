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
            string[] datums = Program.Files.ReadText("info/textures.dat").Split('\n');
            List<MaterialTextureInfo> texs = new List<MaterialTextureInfo>();
            for (int i = 0; i < datums.Length; i++)
            {
                if (datums[i].StartsWith("#") || datums[i].Length <= 1)
                {
                    continue;
                }
                MaterialTextureInfo tex = new MaterialTextureInfo();
                string[] dets = datums[i].Split('=');
                if (dets[0].StartsWith("m"))
                {
                    tex.Mat = (Material)(MaterialHelpers.MAX_TEXTURES - Utilities.StringToInt(dets[0].Substring(1)));
                }
                else
                {
                    tex.Mat = MaterialHelpers.FromNameOrNumber(dets[0]);
                }
                string[] reflecornot = dets[1].Split('*');
                if (reflecornot.Length > 1)
                {
                    tex.Reflectivity = Utilities.StringToFloat(reflecornot[1]);
                }
                string[] specornot = reflecornot[0].Split('&');
                if (specornot.Length > 1)
                {
                    tex.Specular = Utilities.StringToFloat(specornot[1]);
                }
                string[] rateornot = specornot[0].Split('%');
                if (rateornot.Length > 1)
                {
                    tex.Rate = Utilities.StringToFloat(rateornot[1]);
                }
                tex.Textures = rateornot[0].Split(',');
                SetTexture((int)tex.Mat, tex.Textures[0]);
                if (tex.Textures.Length > 1)
                {
                    SetAnimated((int)tex.Mat, tex.Rate, tex.Textures);
                }
                texs.Add(tex);
            }
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            HelpTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, HelpTextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.R8, 2, 2, MaterialHelpers.MAX_TEXTURES);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            for (int i = 0; i < texs.Count; i++)
            {
                SetSettings((int)texs[i].Mat, texs[i].Specular, 0, texs[i].Reflectivity);
            }
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        private void SetSettings(int id, float specular, float waviness, float reflectivity)
        {
            float[] set = new float[] { specular, waviness, reflectivity, 0 };
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

    public class MaterialTextureInfo
    {
        public Material Mat;

        public string[] Textures;

        public double Rate = 1;

        public float Specular = 0;

        public float Reflectivity = 0;
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
