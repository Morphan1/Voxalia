using System.Runtime.InteropServices;

namespace Voxalia.Shared
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct BlockInternal
    {
        public static BlockInternal AIR = new BlockInternal(0, 0, 0, 0);

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
    }
}
