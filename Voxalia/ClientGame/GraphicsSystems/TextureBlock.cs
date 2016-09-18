using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using FreneticScript;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class TextureBlock
    {
        public Client TheClient;

        public TextureEngine TEngine;

        public List<AnimatedTexture> Anims;

        public int TextureID = -1;
        
        public int NormalTextureID = -1;

        public int HelpTextureID = -1;

        public int TWidth;

        /// <summary>
        /// TODO: Direct links, not lookup strings!
        /// </summary>
        public string[] IntTexs;

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
                GL.DeleteTexture(NormalTextureID);
                GL.DeleteTexture(HelpTextureID);
            }
            Anims = new List<AnimatedTexture>();
            TEngine = eng;
            TextureID = GL.GenTexture();
            TWidth = cvars.r_blocktexturewidth.ValueI;
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, TWidth, TWidth, MaterialHelpers.TextureCount);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)(cvars.r_blocktexturelinear.ValueB ? TextureMinFilter.Linear: TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)(cvars.r_blocktexturelinear.ValueB ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            HelpTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, HelpTextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, TWidth, TWidth, MaterialHelpers.TextureCount);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            NormalTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, NormalTextureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, TWidth, TWidth, MaterialHelpers.TextureCount);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // TODO: Use normal.a!
            string[] datums = TheClient.Files.ReadText("info/textures.dat").SplitFast('\n');
            // TODO: Separate files for each texture detail!
            List<MaterialTextureInfo> texs = new List<MaterialTextureInfo>(datums.Length);
            IntTexs = new string[MaterialHelpers.TextureCount];
            float time = 0;
            for (int ia = 0; ia < datums.Length; ia++)
            {
                int i = ia;
                if (datums[i].StartsWith("#") || datums[i].Length <= 1)
                {
                    continue;
                }
                // TODO: Make this saner, and don't allow entering a game until it's done maybe?
                //TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    MaterialTextureInfo tex = new MaterialTextureInfo();
                    string[] dets = datums[i].SplitFast('=');
                    if (dets[0].StartsWith("m"))
                    {
                        tex.Mat = (Material)(MaterialHelpers.TextureCount - Utilities.StringToInt(dets[0].Substring(1)));
                    }
                    else
                    {
                        tex.Mat = MaterialHelpers.FromNameOrNumber(dets[0]);
                    }
                    string[] refrornot = dets[1].SplitFast('@');
                    if (refrornot.Length > 1)
                    {
                        string[] rorn = refrornot[1].SplitFast('%');
                        if (rorn.Length > 1)
                        {
                            tex.RefrRate = Utilities.StringToFloat(rorn[1]);
                        }
                        tex.RefractTextures = rorn[0].SplitFast(',');
                    }
                    string[] glowornot = refrornot[0].SplitFast('!');
                    if (glowornot.Length > 1)
                    {
                        tex.GlowingTextures = glowornot[1].SplitFast(',');
                    }
                    string[] reflornot = glowornot[0].SplitFast('*');
                    if (reflornot.Length > 1)
                    {
                        tex.ReflectTextures = reflornot[1].SplitFast(',');
                    }
                    string[] specularornot = reflornot[0].SplitFast('&');
                    if (specularornot.Length > 1)
                    {
                        tex.SpecularTextures = specularornot[1].SplitFast(',');
                    }
                    string[] normalornot = specularornot[0].SplitFast('$');
                    GL.BindTexture(TextureTarget.Texture2DArray, NormalTextureID);
                    if (normalornot.Length > 1)
                    {
                        string[] rorn = normalornot[1].SplitFast('%');
                        if (rorn.Length > 1)
                        {
                            tex.NormRate = Utilities.StringToFloat(rorn[1]);
                        }
                        tex.NormalTextures = rorn[0].SplitFast(',');
                        SetTexture((int)tex.Mat, tex.NormalTextures[0]);
                        if (tex.NormalTextures.Length > 1)
                        {
                            SetAnimated((int)tex.Mat, tex.NormRate, tex.NormalTextures, NormalTextureID);
                        }
                    }
                    else
                    {
                        SetTexture((int)tex.Mat, "normal_def");
                    }
                    string[] rateornot = normalornot[0].SplitFast('%');
                    if (rateornot.Length > 1)
                    {
                        tex.Rate = Utilities.StringToFloat(rateornot[1]);
                    }
                    tex.Textures = rateornot[0].SplitFast(',');
                    GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
                    SetTexture((int)tex.Mat, tex.Textures[0]);
                    if (tex.Textures.Length > 1)
                    {
                        SetAnimated((int)tex.Mat, tex.Rate, tex.Textures, TextureID);
                    }
                    texs.Add(tex);
                    IntTexs[(int)tex.Mat] = tex.Textures[0];
                }//, i * LoadRate);
                time = i * 0.1f;
            }
            for (int ia = 0; ia < texs.Count; ia++)
            {
                int i = ia;
                //TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    GL.BindTexture(TextureTarget.Texture2DArray, HelpTextureID);
                    Bitmap combo = GetCombo(texs[i], 0);
                    TEngine.LockBitmapToTexture(combo, (int)texs[i].Mat);
                    if ((texs[i].SpecularTextures != null) && (texs[i].ReflectTextures != null) && (texs[i].RefractTextures != null) && (texs[i].GlowingTextures != null) && texs[i].SpecularTextures.Length > 1)
                    {
                        Bitmap[] bmps = new Bitmap[texs[i].SpecularTextures.Length];
                        bmps[0] = combo;
                        for (int x = 1; x < bmps.Length; x++)
                        {
                            bmps[x] = GetCombo(texs[i], x);
                        }
                        SetAnimated((int)texs[i].Mat, texs[i].RefrRate, bmps, HelpTextureID);
                        for (int x = 1; x < bmps.Length; x++)
                        {
                            bmps[x].Dispose();
                        }
                    }
                    combo.Dispose();
                }//, time + i * LoadRate);
            }
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        const float LoadRate = 0.1f;

        public Bitmap GetCombo(MaterialTextureInfo tex, int coord)
        {
            string refract = (tex.RefractTextures != null && tex.RefractTextures.Length > coord) ? tex.RefractTextures[coord] : "black";
            Texture trefr = TEngine.GetTexture(refract, TWidth);
            Bitmap bmprefr = trefr.SaveToBMP();
            string reflect = (tex.ReflectTextures != null && tex.ReflectTextures.Length > coord) ? tex.ReflectTextures[coord] : "black";
            Texture trefl = TEngine.GetTexture(reflect, TWidth);
            Bitmap bmprefl = trefl.SaveToBMP();
            string specular = (tex.SpecularTextures != null && tex.SpecularTextures.Length > coord) ? tex.SpecularTextures[coord] : "black";
            Texture tspec = TEngine.GetTexture(specular, TWidth);
            Bitmap bmpspec = tspec.SaveToBMP();
            string glowing = (tex.GlowingTextures != null && tex.GlowingTextures.Length > coord) ? tex.GlowingTextures[coord] : "black";
            Texture tglow = TEngine.GetTexture(glowing, TWidth);
            Bitmap bmpglow = tglow.SaveToBMP();
            Bitmap combo = Combine(bmpspec, bmprefl, bmprefr, bmpglow);
            bmprefr.Dispose();
            bmprefl.Dispose();
            bmpspec.Dispose();
            bmpglow.Dispose();
            return combo;
        }

        public Bitmap Combine(Bitmap one, Bitmap two, Bitmap three, Bitmap four)
        {
            Bitmap combined = new Bitmap(TWidth, TWidth);
            for (int x = 0; x < TWidth; x++)
            {
                for (int y = 0; y < TWidth; y++)
                {
                    combined.SetPixel(x, y, Color.FromArgb(Gray(four.GetPixel(x, y)), Gray(one.GetPixel(x, y)), Gray(two.GetPixel(x, y)), Gray(three.GetPixel(x, y))));
                }
            }
            return combined;
        }

        public int Gray(Color col)
        {
            return (col.R + (int)col.G + col.B) / 3;
        }
        
        public void SetTexture(int ID, string texture)
        {
            TEngine.LoadTextureIntoArray(texture, ID, TWidth);
        }

        public void SetAnimated(int ID, double rate, Bitmap[] textures, int tid)
        {
            AnimatedTexture anim = new AnimatedTexture();
            anim.Block = this;
            anim.Level = ID;
            anim.Time = 0;
            anim.Rate = rate;
            anim.Textures = new int[textures.Length];
            anim.FBOs = new int[textures.Length];
            anim.OwnsTheTextures = true;
            for (int i = 0; i < textures.Length; i++)
            {
                anim.Textures[i] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, anim.Textures[i]);
                TEngine.LockBitmapToTexture(textures[i], false);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                anim.FBOs[i] = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, anim.FBOs[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, anim.Textures[i], 0);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            anim.Current = 0;
            anim.FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, anim.FBO);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tid, 0, ID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Anims.Add(anim);
        }

        public void SetAnimated(int ID, double rate, string[] textures, int tid)
        {
            AnimatedTexture anim = new AnimatedTexture();
            anim.Block = this;
            anim.Level = ID;
            anim.Time = 0;
            anim.Rate = rate;
            anim.Textures = new int[textures.Length];
            anim.FBOs = new int[textures.Length];
            anim.OwnsTheTextures = false;
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
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tid, 0, ID);
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

        public string[] NormalTextures;

        public string[] RefractTextures;

        public string[] ReflectTextures;

        public string[] SpecularTextures;

        public string[] GlowingTextures;

        public double Rate = 1;

        public double NormRate = 1;

        public double RefrRate = 1;
    }

    public class AnimatedTexture
    {
        public TextureBlock Block;
        
        public int Level;

        public double Rate;

        public double Time;

        public int FBO;

        public int[] FBOs;

        public bool OwnsTheTextures;
        
        public void Tick(double ttime)
        {
            Time += ttime;
            bool changed = false;
            while (Time > Rate)
            {
                Current++;
                if (Current >= Textures.Length)
                {
                    Current = 0;
                }
                Time -= Rate;
                changed = true;
            }
            if (changed)
            {
                AnimateNext();
            }
        }

        public int[] Textures;

        public int Current = 0;

        private void AnimateNext()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FBOs[Current]);
            GL.BlitFramebuffer(0, 0, Block.TWidth, Block.TWidth, 0, 0, Block.TWidth, Block.TWidth, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }

        public void Destroy()
        {
            GL.DeleteFramebuffer(FBO);
            for (int i = 0; i < FBOs.Length; i++)
            {
                GL.DeleteFramebuffer(FBOs[i]);
            }
            if (OwnsTheTextures)
            {
                for (int i = 0; i < Textures.Length; i++)
                {
                    GL.DeleteTexture(Textures[i]);
                }
            }
        }
    }
}
