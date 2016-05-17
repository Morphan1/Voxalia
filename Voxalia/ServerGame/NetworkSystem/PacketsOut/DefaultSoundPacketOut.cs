using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class DefaultSoundPacketOut: AbstractPacketOut
    {
        public DefaultSoundPacketOut(Location loc, DefaultSound sound, byte subdat)
        {
            ID = ServerToClientPacket.DEFAULT_SOUND;
            Data = new byte[12 + 1 + 1];
            loc.ToBytes().CopyTo(Data, 0);
            Data[12] = (byte)sound;
            Data[12 + 1] = subdat;
        }
    }
}
