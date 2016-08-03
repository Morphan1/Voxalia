using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using Voxalia.Shared;
using FreneticScript;
using System.Linq;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class GLFontEngine
    {
        public GLFontEngine(ShaderEngine sengine)
        {
            Shaders = sengine;
        }

        public ShaderEngine Shaders;

        /// <summary>
        /// The default font.
        /// </summary>
        public GLFont Standard;

        /// <summary>
        /// A full list of loaded GLFonts.
        /// </summary>
        public List<GLFont> Fonts;

        public Client TheClient;

        /// <summary>
        /// Prepares the font system.
        /// </summary>
        public void Init(Client tclient)
        {
            TheClient = tclient;
            if (Fonts != null)
            {
                for (int i = 0; i < Fonts.Count; i++)
                {
                    Fonts[i].Remove();
                    i--;
                }
            }
            // Generate the texture array
            GL.GenTextures(1, out Texture3D);
            GL.BindTexture(TextureTarget.Texture2DArray, Texture3D);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 8, SizedInternalFormat.Rgba8, bwidth, bheight, bdepth);
            // Load other stuff
            LoadTextFile();
            Fonts = new List<GLFont>();
            // Choose a default font.
            FontFamily[] families = FontFamily.Families;
            FontFamily family = FontFamily.GenericMonospace;
            int family_priority = 0;
            string fname = "sourcecodepro";
            try
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile("data/fonts/" + fname + ".ttf");
                family = pfc.Families[0];
                family_priority = 100;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.WARNING, "Loading " + fname + ": " + ex.ToString());
            }
            for (int i = 0; i < families.Length; i++)
            {
                if (family_priority < 20 && families[i].Name.ToLowerFast() == "dejavu serif")
                {
                    family = families[i];
                    family_priority = 20;
                }
                else if (family_priority < 10 && families[i].Name.ToLowerFast() == "segoe ui")
                {
                    family = families[i];
                    family_priority = 10;
                }
                else if (family_priority < 5 && families[i].Name.ToLowerFast() == "arial")
                {
                    family = families[i];
                    family_priority = 5;
                }
                else if (family_priority < 2 && families[i].Name.ToLowerFast() == "calibri")
                {
                    family = families[i];
                    family_priority = 2;
                }
            }
            Font def = new Font(family, 12);
            Standard = new GLFont(def, this);
            Fonts.Add(Standard);
        }

        /// <summary>
        /// The text file string to base letters on.
        /// </summary>
        public string textfile;

        /// <summary>
        /// Loads the character list file.
        /// </summary>
        public void LoadTextFile()
        {
            textfile = "";
            string[] datas;
            if (TheClient.Files.Exists("info/characters.dat"))
            {
                datas = TheClient.Files.ReadText("info/characters.dat").Replace("\r", "").SplitFast('\n');
            }
            else
            {
                datas = new string[] { " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=~`[]{};:'\",./<>?\\| " };
            }
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].Length > 0 && !datas[i].StartsWith("//"))
                {
                    textfile += datas[i];
                }
            }
            string tempfile = "?";
            for (int i = 0; i < textfile.Length; i++)
            {
                if (!tempfile.Contains(textfile[i]))
                {
                    tempfile += textfile[i].ToString();
                }
            }
            textfile = tempfile;
        }

        public uint Texture3D;

        /// <summary>
        /// Gets the font matching the specified settings.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="bold">Whether it's bold.</param>
        /// <param name="italic">Whether it's italic.</param>
        /// <param name="size">The font size.</param>
        /// <returns>A valid font object.</returns>
        public GLFont GetFont(string name, bool bold, bool italic, int size)
        {
            string namelow = name.ToLowerFast();
            for (int i = 0; i < Fonts.Count; i++)
            {
                if (Fonts[i].Name.ToLowerFast() == namelow && bold == Fonts[i].Bold && italic == Fonts[i].Italic && size == Fonts[i].Size)
                {
                    return Fonts[i];
                }
            }
            GLFont Loaded = LoadFont(name, bold, italic, size);
            if (Loaded == null)
            {
                return Standard;
            }
            Fonts.Add(Loaded);
            return Loaded;
        }

        /// <summary>
        /// Loads a font matching the specified settings.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="bold">Whether it's bold.</param>
        /// <param name="italic">Whether it's italic.</param>
        /// <param name="size">The font size.</param>
        /// <returns>A valid font object, or null if there was no match.</returns>
        public GLFont LoadFont(string name, bool bold, bool italic, int size)
        {
            Font font = new Font(name, size, (bold ? FontStyle.Bold : 0) | (italic ? FontStyle.Italic : 0));
            return new GLFont(font, this);
        }

        public int bwidth = 512;
        public int bheight = 512;
        public int bdepth = 48;
    }

    /// <summary>
    /// A class for rendering text within OpenGL.
    /// </summary>
    public class GLFont
    {
        public GLFontEngine Engine;

        /// <summary>
        /// The texture containing all character images.
        /// </summary>
        public Texture BaseTexture;

        /// <summary>
        /// A list of all supported characters.
        /// </summary>
        public string Characters;

        /// <summary>
        /// A list of all character locations on the base texture.
        /// </summary>
        public List<RectangleF> CharacterLocations;

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name;

        /// <summary>
        /// The size of the font.
        /// </summary>
        public int Size;

        /// <summary>
        /// Whether the font is bold.
        /// </summary>
        public bool Bold;

        /// <summary>
        /// Whether the font is italic.
        /// </summary>
        public bool Italic;

        /// <summary>
        /// The font used to create this GLFont.
        /// </summary>
        public Font Internal_Font;

        /// <summary>
        /// How tall a rendered symbol is.
        /// </summary>
        public float Height;

        static int cZ = 0;

        public GLFont(Font font, GLFontEngine eng)
        {
            Engine = eng;
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Engine.Shaders.ColorMultShader.Bind();
            Name = font.Name;
            Size = (int)font.Size;
            Bold = font.Bold;
            Italic = font.Italic;
            Height = font.Height;
            CharacterLocations = new List<RectangleF>();
            StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
            sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap;
            Internal_Font = font;
            Bitmap bmp = new Bitmap(Engine.bwidth, Engine.bheight);
            GL.BindTexture(TextureTarget.Texture2DArray, Engine.Texture3D);
            Characters = Engine.textfile;
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.Transparent);
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                float X = 6;
                float Y = 0;
                gfx.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, 5, (int)Height));
                Brush brush = new SolidBrush(Color.White);
                for (int i = 0; i < Engine.textfile.Length; i++)
                {
                    string chr = Engine.textfile[i] == '\t' ? "    " : Engine.textfile[i].ToString();
                    float nwidth = (float)Math.Ceiling(gfx.MeasureString(chr, font, new PointF(0, 0), sf).Width);
                    if (font.Italic)
                    {
                        nwidth += 2;
                    }
                    if (X + nwidth >= Engine.bwidth)
                    {
                        Y += Height + 8;
                        X = 6;
                    }
                    gfx.DrawString(chr, font, brush, new PointF(X, Y), sf);
                    CharacterLocations.Add(new RectangleF(X, Y, nwidth, Height));
                    X += nwidth + 8f;
                }
            }
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, Engine.bwidth, Engine.bheight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, cZ, Engine.bwidth, Engine.bheight, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            TexZ = cZ;
            cZ++;
            cZ = cZ % 48;
            // TODO: Handle > 24 fonts more cleanly
            bmp.UnlockBits(data);
            bmp.Dispose();
        }

        /// <summary>
        /// Removes the GLFont.
        /// </summary>
        public void Remove()
        {
            Engine.Fonts.Remove(this);
        }

        /// <summary>
        /// Gets the location of a symbol.
        /// </summary>
        /// <param name="symbol">The symbol to find.</param>
        /// <returns>A rectangle containing the precise location of a symbol.</returns>
        public RectangleF RectForSymbol(char symbol)
        {
            // TODO: This is eating power. Add special cases for common symbols!
            int loc = Characters.IndexOf(symbol);
            if (loc < 0)
            {
                return CharacterLocations[0];
            }
            return CharacterLocations[loc];
        }

        public int TexZ = 0;

        /// <summary>
        /// Draws a single symbol at a specified location.
        /// </summary>
        /// <param name="symbol">The symbol to draw..</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <returns>The length of the character in pixels.</returns>
        public float DrawSingleCharacter(char symbol, float X, float Y, bool flip, TextVBO vbo, Vector4 color)
        {
            RectangleF rec = RectForSymbol(symbol);
            if (flip)
            {
                vbo.AddQuad(new Vector2(X, Y),
                    new Vector2(X + rec.Width, Y + rec.Height),
                    new Vector2(rec.X / Engine.bwidth, (rec.Y + rec.Height) / Engine.bheight),
                    new Vector2((rec.X + rec.Width) / Engine.bwidth, rec.Y / Engine.bheight), color, TexZ);
            }
            else
            {
                vbo.AddQuad(new Vector2(X, Y),
                    new Vector2(X + rec.Width, Y + rec.Height),
                    new Vector2(rec.X / Engine.bwidth, rec.Y / Engine.bwidth),
                    new Vector2((rec.X + rec.Width) / Engine.bheight, (rec.Y + rec.Height) / Engine.bheight), color, TexZ);
            }
            return rec.Width;
        }

        /// <summary>
        /// Draws a string at a specified location.
        /// </summary>
        /// <param name="str">The string to draw..</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <returns>The length of the string in pixels.</returns>
        public float DrawString(string str, float X, float Y, Vector4 color, TextVBO vbo, bool flip = false)
        {
            float nX = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    Y += Height;
                    nX = 0;
                }
                nX += DrawSingleCharacter(str[i], X + nX, Y, flip, vbo, color);
            }
            return nX;
        }

        /// <summary>
        /// Measures the drawn length of a string.
        /// </summary>
        /// <param name="str">The string to measure.</param>
        /// <returns>The length of the string.</returns>
        public float MeasureString(string str)
        {
            float X = 0;
            for (int i = 0; i < str.Length; i++)
            {
                X += RectForSymbol(str[i]).Width;
            }
            return X;
        }
    }
}
