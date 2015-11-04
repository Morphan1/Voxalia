using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Frenetic;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class TextureEngine
    {
        /// <summary>
        /// What texture widths/heights are allowed.
        /// </summary>
        public static int[] AcceptableWidths = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        /// <summary>
        /// A full list of currently loaded textures.
        /// TODO: List->Dictionary?
        /// </summary>
        public List<Texture> LoadedTextures = null;

        /// <summary>
        /// A default white texture.
        /// </summary>
        public Texture White = null;

        /// <summary>
        /// A default clear texture.
        /// </summary>
        public Texture Clear = null;

        /// <summary>
        /// A default black texture.
        /// </summary>
        public Texture Black = null;

        /// <summary>
        /// An empty bitmap, for regular use.
        /// </summary>
        public Bitmap EmptyBitmap = null;

        /// <summary>
        /// A single graphics object for regular use.
        /// </summary>
        public Graphics GenericGraphicsObject = null;

        /// <summary>
        /// Starts or restarts the texture system.
        /// </summary>
        public void InitTextureSystem()
        {
            // Create a generic graphics object for later use
            EmptyBitmap = new Bitmap(1, 1);
            GenericGraphicsObject = Graphics.FromImage(EmptyBitmap);
            // Reset texture list
            LoadedTextures = new List<Texture>();
            // Pregenerate a few needed textures
            White = GenerateForColor(Color.White, "white");
            LoadedTextures.Add(White);
            Black = GenerateForColor(Color.Black, "black");
            LoadedTextures.Add(Black);
            Clear = GenerateForColor(Color.Transparent, "clear");
            LoadedTextures.Add(Clear);
        }

        public void Empty()
        {
            for (int i = 0; i < LoadedTextures.Count; i++)
            {
                LoadedTextures[i].Destroy();
                LoadedTextures[i].Internal_Texture = -1;
                LoadedTextures[i].Original_InternalID = -1;
            }
            LoadedTextures.Clear();
        }

        public void Update(double time)
        {
            cTime = time;
        }

        public double cTime = 0;

        /// <summary>
        /// Gets the texture object for a specific texture name.
        /// </summary>
        /// <param name="texturename">The name of the texture.</param>
        /// <returns>A valid texture object.</returns>
        public Texture GetTexture(string texturename)
        {
            texturename = FileHandler.CleanFileName(texturename);
            for (int i = 0; i < LoadedTextures.Count; i++)
            {
                if (LoadedTextures[i].Name == texturename)
                {
                    return LoadedTextures[i];
                }
            }
            Texture Loaded = LoadTexture(texturename);
            if (Loaded == null)
            {
                Loaded = new Texture();
                Loaded.Engine = this;
                Loaded.Name = texturename;
                Loaded.Internal_Texture = White.Original_InternalID;
                Loaded.Original_InternalID = White.Original_InternalID;
                Loaded.LoadedProperly = false;
            }
            LoadedTextures.Add(Loaded);
            if (OnTextureLoaded != null)
            {
                OnTextureLoaded(this, new TextureLoadedEventArgs(Loaded));
            }
            return Loaded;
        }

        public event EventHandler<TextureLoadedEventArgs> OnTextureLoaded;

        /// <summary>
        /// Loads a texture from file.
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <returns>The loaded texture, or null if it does not exist.</returns>
        public Texture LoadTexture(string filename, int twidth = 0)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!Program.Files.Exists("textures/" + filename + ".png"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load texture, file '" +
                        TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error +
                        "' does not exist.");
                    return null;
                }
                Bitmap bmp = new Bitmap(Program.Files.ReadToStream("textures/" + filename + ".png"));
                if (!AcceptableWidths.Contains(bmp.Width) || !AcceptableWidths.Contains(bmp.Height))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load texture, file '" +
                        TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error +
                        "' is invalid: unreasonable width or height.");
                    return null;
                }
                Bitmap bmp2 = twidth <= 0 ? bmp : new Bitmap(bmp, new Size(twidth, twidth));
                Texture texture = new Texture();
                texture.Engine = this;
                texture.Name = filename;
                GL.GenTextures(1, out texture.Original_InternalID);
                texture.Internal_Texture = texture.Original_InternalID;
                texture.Bind();
                LockBitmapToTexture(bmp2);
                texture.Width = bmp2.Width;
                texture.Height = bmp2.Height;
                if (bmp2 != bmp)
                {
                    bmp2.Dispose();
                }
                bmp.Dispose();
                texture.LoadedProperly = true;
                return texture;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load texture from filename '" +
                    TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error + "': " + ex.ToString());
                return null;
            }
        }

        public int GetTextureID(string name, int twidth = 0)
        {
            return (LoadTexture(name, twidth) ?? LoadTexture("white", twidth)).Original_InternalID;
        }

        /// <summary>
        /// Loads a texture from file.
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <param name="depth">Where in the array to put it.</param>
        /// <param name="twidth">What width the texture must be at.</param>
        /// <returns>The loaded texture, or null if it does not exist.</returns>
        public void LoadTextureIntoArray(string filename, int depth, int twidth)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!Program.Files.Exists("textures/" + filename + ".png"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load texture, file '" +
                        TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error +
                        "' does not exist.");
                    return;
                }
                Bitmap bmp = new Bitmap(Program.Files.ReadToStream("textures/" + filename + ".png"));
                Bitmap bmp2 = new Bitmap(bmp, new Size(twidth, twidth));
                LockBitmapToTexture(bmp2, depth);
                bmp2.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load texture from filename '" +
                    TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error + "': " + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Creates a Texture object for a specific color.
        /// </summary>
        /// <param name="c">The color to use.</param>
        /// <param name="name">The name of the texture.</param>
        /// <returns>The generated texture.</returns>
        public Texture GenerateForColor(Color c, string name)
        {
            Texture texture = new Texture();
            texture.Engine = this;
            texture.Name = name;
            GL.GenTextures(1, out texture.Original_InternalID);
            texture.Internal_Texture = texture.Original_InternalID;
            texture.Bind();
            texture.Width = 2;
            texture.Height = 2;
            Bitmap bmp = new Bitmap(2, 2);
            bmp.SetPixel(0, 0, c);
            bmp.SetPixel(0, 1, c);
            bmp.SetPixel(1, 0, c);
            bmp.SetPixel(1, 1, c);
            LockBitmapToTexture(bmp);
            bmp.Dispose();
            texture.LoadedProperly = true;
            return texture;
        }

        /// <summary>
        /// Locks a bitmap file's data to a GL texture.
        /// </summary>
        /// <param name="bmp">The bitmap to use.</param>
        public void LockBitmapToTexture(Bitmap bmp)
        {
            // Send the bits across
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
            // Disable mipmapping
            // TODO: Enable mipmapping optionally?
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
        }

        /// <summary>
        /// Locks a bitmap file's data to a GL texture array.
        /// </summary>
        /// <param name="bmp">The bitmap to use.</param>
        public void LockBitmapToTexture(Bitmap bmp, int depth)
        {
            // Send the bits across
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, depth, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
        }

    }

    public class TextureLoadedEventArgs : EventArgs
    {
        public TextureLoadedEventArgs(Texture t)
        {
            Tex = t;
        }

        public Texture Tex;
    }

    /// <summary>
    /// Wraps an OpenGL texture.
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// The texture engine that owns this texture.
        /// </summary>
        public TextureEngine Engine;

        /// <summary>
        /// The full name of the texture.
        /// </summary>
        public string Name;

        /// <summary>
        /// The texture that this texture was remapped to, if any.
        /// </summary>
        public Texture RemappedTo;

        /// <summary>
        /// The internal OpenGL texture ID.
        /// </summary>
        public int Internal_Texture = 0;

        /// <summary>
        /// The original OpenGL texture ID that formed this texture.
        /// </summary>
        public int Original_InternalID = 0;

        /// <summary>
        /// Whether the texture loaded properly.
        /// </summary>
        public bool LoadedProperly = false;

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height;

        /// <summary>
        /// Removes the texture from OpenGL.
        /// </summary>
        public void Destroy()
        {
            if (Original_InternalID > -1 && GL.IsTexture(Original_InternalID))
            {
                GL.DeleteTexture(Original_InternalID);
            }
        }

        /// <summary>
        /// Removes the texture from the system.
        /// </summary>
        public void Remove()
        {
            Destroy();
            Engine.LoadedTextures.Remove(this);
        }

        /// <summary>
        /// Saves the texture to a bitmap.
        /// </summary>
        /// <param name="flip">Whether to flip the Y.</param>
        public Bitmap SaveToBMP(bool flip = false)
        {
            GL.BindTexture(TextureTarget.Texture2D, Original_InternalID);
            Bitmap bmp = new Bitmap(Width, Height);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            if (flip)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return bmp;
        }

        public double LastBindTime = 0;

        public void CheckValid()
        {
            if (Internal_Texture == -1)
            {
                Texture temp = Engine.GetTexture(Name);
                Original_InternalID = temp.Original_InternalID;
                Internal_Texture = Original_InternalID;
                if (RemappedTo != null)
                {
                    RemappedTo.CheckValid();
                    Internal_Texture = RemappedTo.Original_InternalID;
                }
            }
        }

        /// <summary>
        /// Binds this texture to OpenGL.
        /// </summary>
        public void Bind()
        {
            LastBindTime = Engine.cTime;
            CheckValid();
            GL.BindTexture(TextureTarget.Texture2D, Internal_Texture);
        }
    }
}
