using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class AddCloudPacketOut: AbstractPacketOut
    {
        public AddCloudPacketOut(Cloud cloud)
        {
            UsageType = NetUsageType.CLOUDS;
            ID = ServerToClientPacket.ADD_CLOUD;
            DataStream ds = new DataStream();
            DataWriter dw = new DataWriter(ds);
            dw.WriteBytes(cloud.Position.ToBytes());
            dw.WriteBytes((cloud.Velocity + cloud.TheRegion.Wind).ToBytes());
            dw.WriteLong(cloud.CID);
            dw.WriteInt(cloud.Points.Count);
            for (int i = 0; i < cloud.Points.Count; i++)
            {
                dw.WriteBytes(cloud.Points[i].ToBytes());
                dw.WriteFloat(cloud.Sizes[i]);
                dw.WriteFloat(cloud.EndSizes[i]);
            }
            dw.Flush();
            Data = ds.ToArray();
            dw.Close();
        }
    }
}
