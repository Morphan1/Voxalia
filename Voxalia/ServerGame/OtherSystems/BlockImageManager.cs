using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using FreneticScript;
using BEPUutilities;

namespace Voxalia.ServerGame.OtherSystems
{
    public class MaterialImage
    {
        public Color[,] Colors;
    }

    public class BlockImageManager
    {
        public const int TexWidth = 4;
        public const int TexWidth2 = TexWidth * 2;
        const int BmpSize2 = TexWidth2 * Chunk.CHUNK_SIZE;

        public MaterialImage[] MaterialImages;

        public Object OneAtATimePlease = new Object();

        static readonly Color Transp = Color.FromArgb(0, 0, 0, 0);

        public void RenderChunk(WorldSystem.Region tregion, Vector3i chunkCoords, Chunk chunk)
        {
            RenderChunkInternal(tregion, chunkCoords, chunk);
            if (tregion.TheServer.CVars.n_rendersides.ValueB)
            {
                RenderChunkInternalAngle(tregion, chunkCoords, chunk);
            }
        }

        public byte[] Combine(List<byte[]> originals, bool angle)
        {
            Bitmap bmp = new Bitmap(Chunk.CHUNK_SIZE * (angle ? TexWidth2 : TexWidth), Chunk.CHUNK_SIZE * (angle ? TexWidth2 : TexWidth), PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(Transp);
                for (int i = 0; i < originals.Count; i++)
                {
                    DataStream ds = new DataStream(originals[i]);
                    Bitmap tbmp = new Bitmap(ds);
                    graphics.DrawImage(tbmp, 0, 0);
                    tbmp.Dispose();
                }
            }
            DataStream temp = new DataStream();
            bmp.Save(temp, ImageFormat.Png);
            return temp.ToArray();
        }

        Color Blend(Color one, Color two)
        {
            float a1 = one.A / 255f;
            float a2 = 1f - a1;
            float r = ((one.R / 255f) * a1) + (two.R / 255f) * a2;
            float g = ((one.G / 255f) * a1) + (two.G / 255f) * a2;
            float b = ((one.B / 255f) * a1) + (two.B / 255f) * a2;
            return Color.FromArgb((byte)Math.Min(one.A + (int)two.A, 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        Color Multiply(Color one, Color two)
        {
            return Color.FromArgb((byte)(((one.A / 255f) * (two.A / 255f)) * 255),
                (byte)(((one.R / 255f) * (two.R / 255f)) * 255),
                (byte)(((one.G / 255f) * (two.G / 255f)) * 255),
                (byte)(((one.B / 255f) * (two.B / 255f)) * 255));
        }

        void DrawImage(Bitmap bmp, MaterialImage bmpnew, int xmin, int ymin, Color col)
        {
            for (int x = 0; x < TexWidth; x++)
            {
                for (int y = 0; y < TexWidth; y++)
                {
                    Color basepx = bmp.GetPixel(xmin + x, ymin + y);
                    bmp.SetPixel(xmin + x, ymin + y, Blend(Multiply(bmpnew.Colors[x, y], col), basepx));
                }
            }
        }

        void DrawImageShiftX(Bitmap bmp, MaterialImage bmpnew, int xmin, int ymin, Color col)
        {
            xmin += TexWidth;
            for (int x = 0; x < TexWidth; x++)
            {
                for (int y = 0; y < TexWidth; y++)
                {
                    int sx = xmin + x;
                    int sy = ymin + y - x;
                    if (sx < 0 || sy < 0 || sx >= BmpSize2 || sy >= BmpSize2)
                    {
                        continue;
                    }
                    Color basepx = bmp.GetPixel(sx, sy);
                    bmp.SetPixel(sx, sy, Blend(Multiply(bmpnew.Colors[x, y], col), basepx));
                }
            }
        }

        void DrawImageShiftY(Bitmap bmp, MaterialImage bmpnew, int xmin, int ymin, Color col)
        {
            for (int x = 0; x < TexWidth; x++)
            {
                for (int y = 0; y < TexWidth; y++)
                {
                    int sx = xmin + x;
                    int sy = ymin + y;
                    if (sx < 0 || sy < 0 || sx >= BmpSize2 || sy >= BmpSize2)
                    {
                        continue;
                    }
                    Color basepx = bmp.GetPixel(sx, sy);
                    bmp.SetPixel(sx, sy, Blend(Multiply(bmpnew.Colors[x, y], col), basepx));
                }
            }
        }
        
        void DrawImageShiftZ(Bitmap bmp, MaterialImage bmpnew, int xmin, int ymin, Color col)
        {
            ymin -= TexWidth;
            xmin += TexWidth;
            for (int x = 0; x < TexWidth; x++)
            {
                for (int y = 0; y < TexWidth; y++)
                {
                    int sx = xmin + x - y;
                    int sy = ymin + y;
                    if (sx < 0 || sy < 0 || sx >= BmpSize2 || sy >= BmpSize2)
                    {
                        continue;
                    }
                    Color basepx = bmp.GetPixel(sx, sy);
                    bmp.SetPixel(sx, sy, Blend(Multiply(bmpnew.Colors[x, y], col), basepx));
                }
            }
        }

        const int mid = Chunk.CHUNK_SIZE / 2;

        void RenderBlockIntoAngle(BlockInternal bi, int x, int y, int z, Bitmap bmp)
        {
            MaterialImage zmatbmpXP = MaterialImages[bi.Material.TextureID(MaterialSide.XP)];
            MaterialImage zmatbmpYP = MaterialImages[bi.Material.TextureID(MaterialSide.YP)];
            MaterialImage zmatbmpZP = MaterialImages[bi.Material.TextureID(MaterialSide.TOP)];
            if (zmatbmpXP == null || zmatbmpYP == null || zmatbmpZP == null)
            {
                return;
            }
            Color zcolor = Colors.ForByte(bi.BlockPaint);
            if (zcolor.A == 0)
            {
                zcolor = Color.White;
            }
            int x1 = x * TexWidth;
            int y1 = y * TexWidth;
            int z1 = z * TexWidth;
            //    int xw = x * TexWidth;
            //    int yw = y * TexWidth;
            // tileWidth/2*x+tileHeight/2*y, tileWidth/2*x+tileHeight/2*y
            //   int xw = TexWidth * x / 2 + TexWidth * y / 2;
            //   int yw = TexWidth * x / 2 + TexWidth * y / 2;
            // tempPt.x = pt.x - pt.y; tempPt.y = (pt.x + pt.y) / 2;
            int xw = x1 - y1;
            int yw = ((x1 + y1) - (z1)) / 2;
            //   tempPt.x = (2 * pt.y + pt.x) / 2; tempPt.y = (2 * pt.y - pt.x) / 2;
            // int xw = (2 * y1 + x1) / 2;
            //  int yw = (2 * y1 - x1) / 2;
            xw += BmpSize2 / 2;
            yw += BmpSize2 / 2;
            DrawImageShiftX(bmp, zmatbmpXP, xw, yw, zcolor);
            DrawImageShiftY(bmp, zmatbmpYP, xw, yw, zcolor);
            DrawImageShiftZ(bmp, zmatbmpZP, xw, yw, zcolor);
        }

        void RenderChunkInternalAngle(WorldSystem.Region tregion, Vector3i chunkCoords, Chunk chunk)
        {
            // TODO: Rework into logicalness.
            Bitmap bmp = new Bitmap(Chunk.CHUNK_SIZE * TexWidth2, Chunk.CHUNK_SIZE * TexWidth2, PixelFormat.Format32bppArgb);
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        // TODO: async chunk read locker?
                        BlockInternal bi = chunk.GetBlockAt(x, y, z);
                        if (bi.Material.RendersAtAll())
                        {
                            RenderBlockIntoAngle(bi, x, y, z, bmp);
                        }
                    }
                }
            }
            DataStream ds = new DataStream();
            bmp.Save(ds, ImageFormat.Png);
            tregion.ChunkManager.WriteImageAngle((int)chunkCoords.X, (int)chunkCoords.Y, (int)chunkCoords.Z, ds.ToArray());
        }

        void RenderChunkInternal(WorldSystem.Region tregion, Vector3i chunkCoords, Chunk chunk)
        {
            Bitmap bmp = new Bitmap(Chunk.CHUNK_SIZE * TexWidth, Chunk.CHUNK_SIZE * TexWidth, PixelFormat.Format32bppArgb);
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                {
                    // TODO: async chunk read locker?
                    BlockInternal topOpaque = BlockInternal.AIR;
                    int topZ = 0;
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        BlockInternal bi = chunk.GetBlockAt(x, y, z);
                        if (bi.IsOpaque())
                        {
                            topOpaque = bi;
                            topZ = z;
                        }
                    }
                    if (!topOpaque.Material.RendersAtAll())
                    {
                        DrawImage(bmp, MaterialImages[0], x * TexWidth, y * TexWidth, Color.Transparent);
                    }
                    for (int z = topZ; z < Chunk.CHUNK_SIZE; z++)
                    {
                        BlockInternal bi = chunk.GetBlockAt(x, y, z);
                        if (bi.Material.RendersAtAll())
                        {
                            MaterialImage zmatbmp = MaterialImages[bi.Material.TextureID(MaterialSide.TOP)];
                            if (zmatbmp == null)
                            {
                                continue;
                            }
                            Color zcolor = Colors.ForByte(bi.BlockPaint);
                            if (zcolor.A == 0)
                            {
                                zcolor = Color.White;
                            }
                            DrawImage(bmp, zmatbmp, x * TexWidth, y * TexWidth, zcolor);
                        }
                    }
                }
            }
            DataStream ds = new DataStream();
            bmp.Save(ds, ImageFormat.Png);
            lock (OneAtATimePlease) // NOTE: We can make this grab off an array of locks to reduce load a little.
            {
                KeyValuePair<int, int> maxes = tregion.ChunkManager.GetMaxes((int)chunkCoords.X, (int)chunkCoords.Y);
                tregion.ChunkManager.SetMaxes((int)chunkCoords.X, (int)chunkCoords.Y, Math.Min(maxes.Key, (int)chunkCoords.Z), Math.Max(maxes.Value, (int)chunkCoords.Z));
            }
            tregion.ChunkManager.WriteImage((int)chunkCoords.X, (int)chunkCoords.Y, (int)chunkCoords.Z, ds.ToArray());
        }

        public void Init()
        {
            MaterialImages = new MaterialImage[MaterialHelpers.TextureCount];
            string[] texs = Program.Files.ReadText("info/textures.dat").SplitFast('\n');
            for (int i = 0; i < texs.Length; i++)
            {
                if (texs[i].StartsWith("#") || texs[i].Length <= 1)
                {
                    continue;
                }
                string[] dat = texs[i].SplitFast('=');
                Material mat;
                if (dat[0].StartsWith("m"))
                {
                    mat = (Material)(MaterialHelpers.TextureCount - Utilities.StringToInt(dat[0].Substring(1)));
                }
                else
                {
                    mat = MaterialHelpers.FromNameOrNumber(dat[0]);
                }
                string actualtexture = "textures/" + dat[1].Before(",").Before("&").Before("$").Before("@")+ ".png";
                try
                {
                    Bitmap bmp1 = new Bitmap(Program.Files.ReadToStream(actualtexture));
                    Bitmap bmp2 = new Bitmap(bmp1, new Size(TexWidth, TexWidth));
                    bmp1.Dispose();
                    MaterialImage img = new MaterialImage();
                    img.Colors = new Color[TexWidth, TexWidth];
                    for (int x = 0; x < TexWidth; x++)
                    {
                        for (int y = 0; y < TexWidth; y++)
                        {
                            img.Colors[x, y] = bmp2.GetPixel(x, y);
                        }
                    }
                    MaterialImages[(int)mat] = img;
                    bmp2.Dispose();
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output("loading texture for '" + dat[0] + "': '" + actualtexture + "'", ex);
                }
            }
            SysConsole.Output(OutputType.INIT, "Loaded " + texs.Length + " textures!");
        }
    }
}
