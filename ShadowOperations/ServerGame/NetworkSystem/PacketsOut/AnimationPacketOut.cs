using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    class AnimationPacketOut: AbstractPacketOut
    {
        public AnimationPacketOut(Entity e, string anim, byte mode)
        {
            ID = 17;
            Data = new byte[8 + 4 + 1];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Utilities.IntToBytes(e.TheServer.Networking.Strings.IndexForString(anim)).CopyTo(Data, 8);
            Data[8 + 4] = mode;
        }
    }
}
