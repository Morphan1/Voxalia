using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.WorldSystem
{
    public enum ChunkFlags: int
    {
        NONE = 0,
        ISCUSTOM = 1,
        POPULATING = 2
    }
}
