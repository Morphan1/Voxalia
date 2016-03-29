using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.OtherSystems
{
    public class BlockImageManager
    {
        public const int TexWidth = 4;

        public Bitmap[] MaterialImages;

        public Object OneAtATimePlease = new Object();

        static readonly Color Transp = Color.FromArgb(0, 0, 0, 0);

        public void RenderChunk(WorldSystem.Region tregion, Location chunkCoords, Chunk chunk)
        {
            lock (OneAtATimePlease) // TODO: Less need for this!
            {
                RenderChunkInternal(tregion, chunkCoords, chunk);
            }
        }

        void RenderChunkInternal(WorldSystem.Region tregion, Location chunkCoords, Chunk chunk)
        {
            Bitmap bmp = new Bitmap(Chunk.CHUNK_SIZE * TexWidth, Chunk.CHUNK_SIZE * TexWidth, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(Transp);
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
                        if (topOpaque.Material != Material.AIR)
                        {
                            Bitmap matbmp = MaterialImages[topOpaque.Material.TextureID(MaterialSide.TOP)];
                            if (matbmp == null)
                            {
                                continue;
                            }
                            graphics.DrawImage(matbmp, x * TexWidth, y * TexWidth);
                            for (int z = topZ; z < Chunk.CHUNK_SIZE; z++)
                            {
                                BlockInternal bi = chunk.GetBlockAt(x, y, z);
                                if (bi.Material != Material.AIR)
                                {
                                    Bitmap zmatbmp = MaterialImages[bi.Material.TextureID(MaterialSide.TOP)];
                                    if (zmatbmp == null)
                                    {
                                        continue;
                                    }
                                    graphics.DrawImage(zmatbmp, x * TexWidth, y * TexWidth);
                                }
                            }
                        }
                    }
                }
            }
            DataStream ds = new DataStream();
            bmp.Save(ds, ImageFormat.Png);
            tregion.ChunkManager.WriteImage((int)chunkCoords.X, (int)chunkCoords.Y, (int)chunkCoords.Z, ds.ToArray());
        }

        public void Init()
        {
            MaterialImages = new Bitmap[MaterialHelpers.MAX_THEORETICAL_MATERIALS];
            string[] texs = Program.Files.ReadText("info/textures.dat").Split('\n');
            for (int i = 0; i < texs.Length; i++)
            {
                if (texs[i].StartsWith("#") || texs[i].Length <= 1)
                {
                    continue;
                }
                string[] dat = texs[i].Split('=');
                Material mat;
                if (dat[0].StartsWith("m"))
                {
                    mat = (Material)(MaterialHelpers.MAX_THEORETICAL_MATERIALS - Utilities.StringToInt(dat[0].Substring(1)));
                }
                else
                {
                    mat = MaterialHelpers.FromNameOrNumber(dat[0]);
                }
                string actualtexture = "textures/" + dat[1].Before(",").Before("&").Before("$") + ".png";
                try
                {
                    Bitmap bmp1 = new Bitmap(Program.Files.ReadToStream(actualtexture));
                    Bitmap bmp2 = new Bitmap(bmp1, new Size(TexWidth, TexWidth));
                    bmp1.Dispose();
                    MaterialImages[(int)mat] = bmp2;
                }
                catch (Exception ex)
                {
                    SysConsole.Output("loading texture for '" + dat[0] + "': '" + actualtexture + "'", ex);
                }
            }
            SysConsole.Output(OutputType.INIT, "Loaded " + texs.Length + " textures!");
        }
    }
}
