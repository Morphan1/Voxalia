using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    public class HoverMessageEntity : PrimitiveEntity
    {
        public int Width = 256;

        public Vector4 BackColor = new Vector4(0.25f, 0, 1, 1);

        public string Text;

        public Vector2 Stretch = Vector2.One;

        public HoverMessageEntity(string message, Region tregion) : base(tregion, false)
        {
            Text = message;
        }

        public int GLTexture = 0;

        public int GLFBO = 0;

        public bool NeedsRender = true;

        public override void Destroy()
        {
            if (GLTexture > 0)
            {
                GL.DeleteTexture(GLTexture);
                GL.DeleteFramebuffer(GLFBO);
                GLTexture = 0;
                GLFBO = 0;
            }
        }

        string pText;

        Location size;

        public override void Render()
        {
            if (NeedsRender)
            {
                NeedsRender = false;
                if (Text != pText)
                {
                    Destroy();
                    pText = Text;
                }
                size = TheClient.FontSets.Standard.MeasureFancyLinesOfText(Text);
                if (GLTexture <= 0)
                {
                    GLTexture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, GLTexture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)size.X + 2, (int)size.Y + 2, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                    GLFBO = GL.GenFramebuffer();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFBO);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GLTexture, 0);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFBO);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.Viewport(0, 0, (int)size.X, (int)size.Y);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { BackColor.X, BackColor.Y, BackColor.Z, BackColor.W });
                TheClient.Ortho = Matrix4.CreateOrthographicOffCenter(0, (float)size.X, (float)size.Y, 0, -1, 1);
                TheClient.FontSets.Standard.DrawColoredText("^0" + Text, new Location(1, 1, 0), int.MaxValue, 1, true, "^r^0");
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, TheClient.MainWorldView.cFBO);
                TheClient.MainWorldView.SetViewPort();
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
            }
            NeedsRender = true; // TEMP.
            TheClient.isVox = true;
            TheClient.SetEnts();
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.Disable(EnableCap.CullFace);
            TheClient.Rendering.RenderBillboard(Position, Scale * new Location(size.X * -0.01, size.Y * 0.01, 1f), TheClient.MainWorldView.CameraPos, (float)Math.PI * 0.5f);
            GL.Enable(EnableCap.CullFace);
        }

        public override void Spawn()
        {
        }
    }

    public class HoverMessageEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] e)
        {
            if (e.Length < PrimitiveEntity.PrimitiveNetDataLength + 4 + 4)
            {
                return null;
            }
            DataStream ds = new DataStream(e);
            DataReader dr = new DataReader(ds);
            byte[] t = dr.ReadBytes(PrimitiveEntity.PrimitiveNetDataLength);
            int col = dr.ReadInt();
            string mes = dr.ReadFullString();
            HoverMessageEntity hme = new HoverMessageEntity(mes, tregion);
            System.Drawing.Color tempc = System.Drawing.Color.FromArgb(col);
            hme.BackColor = new Vector4(tempc.R / 255f, tempc.G / 255f, tempc.B / 255f, tempc.A / 255f);
            // TODO: Move Primitive reads to primitive class file?
            hme.Position = Location.FromDoubleBytes(t, 0);
            hme.Velocity = Location.FromDoubleBytes(t, 24);
            hme.Angles = Utilities.BytesToQuaternion(t, 24 + 24);
            hme.Scale = Location.FromDoubleBytes(t, 24 + 24 + 16);
            hme.Gravity = Location.FromDoubleBytes(t, 24 + 24 + 16 + 24);
            return hme;
        }
    }
}
