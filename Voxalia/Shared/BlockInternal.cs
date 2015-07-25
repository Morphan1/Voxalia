using System.Runtime.InteropServices;

namespace Voxalia.Shared
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct BlockInternal
    {
        public static BlockInternal AIR = new BlockInternal(0, 0, 0);

        public ushort BlockMaterial;
        public byte BlockData;
        public byte BlockLocalData;

        public BlockInternal(ushort mat, byte dat, byte locdat)
        {
            BlockMaterial = mat;
            BlockData = dat;
            BlockLocalData = locdat;
        }
    }
}
