using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class FlagEntityPacketOut: AbstractPacketOut
    {
        public FlagEntityPacketOut(Entity e, EntityFlag flag, float value)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.FLAG_ENTITY;
            Data = new byte[8 + 1 + 4];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Data[8] = (byte)flag;
            Utilities.FloatToBytes(value).CopyTo(Data, 8 + 1);
        }
    }
}
