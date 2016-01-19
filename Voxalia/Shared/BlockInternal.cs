using System;
using System.Runtime.InteropServices;

namespace Voxalia.Shared
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct BlockInternal
    {
        public static BlockInternal AIR = new BlockInternal(0, 0, 0, 0);

        public static BlockInternal FromItemDatum(int dat)
        {
            return FromItemDatumU(BitConverter.ToUInt32(BitConverter.GetBytes(dat), 0)); // TODO: Less stupid conversion
        }

        public static BlockInternal FromItemDatumU(uint dat)
        {
            return new BlockInternal((ushort)(dat & (255u | (255u * 256u))), (byte)((dat & (255u * 256u * 256u)) / (256u * 256u)), (byte)((dat & (255u * 256u * 256u * 256u)) / (256u * 256u * 256)), 0);
        }

        public ushort BlockMaterial;
        public byte BlockData;
        public byte BlockPaint;
        public byte BlockLocalData;

        public BlockInternal(ushort mat, byte dat, byte paint, byte loc)
        {
            BlockMaterial = mat;
            BlockData = dat;
            BlockPaint = paint;
            BlockLocalData = loc;
        }

        public bool IsOpaque()
        {
            return ((Material)BlockMaterial).IsOpaque() && BlockPaint != Colors.TRANS1 && BlockPaint != Colors.TRANS2;
        }

        public int GetItemDatum()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(GetItemDatumU()), 0); // TODO: Less stupid conversion
        }

        public uint GetItemDatumU()
        {
            return (uint)BlockMaterial | ((uint)BlockData * 256u * 256u) | ((uint)BlockPaint * 256u * 256u * 256u);
        }
    }
}
