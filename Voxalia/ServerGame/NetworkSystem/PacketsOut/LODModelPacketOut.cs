using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUutilities;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class LODModelPacketOut : AbstractPacketOut
    {
        public LODModelPacketOut(ModelEntity me)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.LOD_MODEL;
            Data = new byte[12 + 4 + 16 + 8 + 12];
            me.GetPosition().ToBytes().CopyTo(Data, 0);
            int ind = me.TheServer.Networking.Strings.IndexForString(me.model);
            Utilities.IntToBytes(ind).CopyTo(Data, 12);
            Quaternion quat = me.GetOrientation();
            Utilities.FloatToBytes(quat.X).CopyTo(Data, 12 + 4);
            Utilities.FloatToBytes(quat.Y).CopyTo(Data, 12 + 4 + 4);
            Utilities.FloatToBytes(quat.Z).CopyTo(Data, 12 + 4 + 8);
            Utilities.FloatToBytes(quat.W).CopyTo(Data, 12 + 4 + 12);
            Utilities.LongToBytes(me.EID).CopyTo(Data, 12 + 4 + 16);
            me.scale.ToBytes().CopyTo(Data, 12 + 4 + 16 + 8);
        }
    }
}
