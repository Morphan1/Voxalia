//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ServerGame.ItemSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnItemPacketOut: AbstractPacketOut
    {
        public SpawnItemPacketOut(int spot, ItemStack item)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.SPAWN_ITEM;
            byte[] itemdat = item.ToBytes();
            Data = new byte[4 + itemdat.Length];
            Utilities.IntToBytes(spot).CopyTo(Data, 0);
            itemdat.CopyTo(Data, 4);
        }
    }
}
