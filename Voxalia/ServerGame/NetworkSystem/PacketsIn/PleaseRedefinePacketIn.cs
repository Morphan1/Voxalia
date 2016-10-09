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
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class PleaseRedefinePacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8)
            {
                return false;
            }
            long eid = Utilities.BytesToLong(data);
            foreach (Entity e in Player.TheRegion.Entities)
            {
                if (e.EID == eid)
                {
                    if (Player.CanSeeChunk(Player.TheRegion.ChunkLocFor(e.GetPosition())))
                    {
                        Player.Network.SendPacket(e.GetSpawnPacket());
                    }
                    break;
                }
            }
            return true;
        }
    }
}
