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
        public AnimationPacketOut(Entity e, string anim)
        {
            ID = 17;
            Data = new byte[8 + 4];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Utilities.IntToBytes(e.TheServer.Networking.Strings.IndexForString(anim)).CopyTo(Data, 8);
        }
    }
}
