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
            Data = new byte[24 + 4 + 16 + 8 + 24];
            me.GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            int ind = me.TheServer.Networking.Strings.IndexForString(me.model);
            Utilities.IntToBytes(ind).CopyTo(Data, 24);
            Quaternion quat = me.GetOrientation();
            Utilities.FloatToBytes((float)quat.X).CopyTo(Data, 24 + 4);
            Utilities.FloatToBytes((float)quat.Y).CopyTo(Data, 24 + 4 + 4);
            Utilities.FloatToBytes((float)quat.Z).CopyTo(Data, 24 + 4 + 4 + 4);
            Utilities.FloatToBytes((float)quat.W).CopyTo(Data, 24 + 4 + 4 + 4 + 4);
            Utilities.LongToBytes(me.EID).CopyTo(Data, 24 + 4 + 16);
            me.scale.ToDoubleBytes().CopyTo(Data, 24 + 4 + 16 + 8);
        }
    }
}
