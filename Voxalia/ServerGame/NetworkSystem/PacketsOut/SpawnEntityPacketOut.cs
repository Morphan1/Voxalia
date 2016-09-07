using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnEntityPacketOut : AbstractPacketOut
    {
        public SpawnEntityPacketOut(Entity e)
        {
            if (!e.NetworkMe)
            {
                throw new ArgumentException("Entity is non-networkable!");
            }
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.SPAWN_ENTITY;
            byte t = (byte)e.GetNetType();
            byte[] dat = e.GetNetData();
            Data = new byte[1 + 8 + dat.Length];
            Data[0] = t;
            Utilities.LongToBytes(e.EID).CopyTo(Data, 1);
            dat.CopyTo(Data, 1 + 8);
        }
    }
}
