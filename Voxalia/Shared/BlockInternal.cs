using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Voxalia.Shared
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct BlockInternal
    {
        public static BlockInternal AIR = new BlockInternal(0, 0);

        public ushort BlockMaterial;
        public byte BlockData;

        public BlockInternal(ushort mat, byte dat)
        {
            BlockMaterial = mat;
            BlockData = dat;
        }
    }
}
