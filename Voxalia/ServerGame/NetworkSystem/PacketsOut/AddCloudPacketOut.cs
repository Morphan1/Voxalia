//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
            dw.WriteBytes(cloud.Position.ToDoubleBytes());
            dw.WriteBytes((cloud.Velocity + cloud.TheRegion.Wind).ToDoubleBytes());
            dw.WriteLong(cloud.CID);
            dw.WriteInt(cloud.Points.Count);
            for (int i = 0; i < cloud.Points.Count; i++)
            {
                dw.WriteBytes(cloud.Points[i].ToDoubleBytes());
                dw.WriteFloat((float)cloud.Sizes[i]);
                dw.WriteFloat((float)cloud.EndSizes[i]);
            }
            dw.Flush();
            Data = ds.ToArray();
            dw.Close();
        }
    }
}
