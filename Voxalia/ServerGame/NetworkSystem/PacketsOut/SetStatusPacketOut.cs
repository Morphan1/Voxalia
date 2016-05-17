using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SetStatusPacketOut: AbstractPacketOut
    {
        public SetStatusPacketOut(PlayerEntity player, ClientStatus opt, byte status)
        {
            ID = ServerToClientPacket.SET_STATUS;
            Data = new byte[8 + 1 + 1];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            Data[8] = (byte)opt;
            Data[8 + 1] = status;
        }
    }
}
