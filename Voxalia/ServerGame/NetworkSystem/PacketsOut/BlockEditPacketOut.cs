//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class BlockEditPacketOut: AbstractPacketOut
    {
        public BlockEditPacketOut(Location[] pos, ushort[] mat, byte[] dat, byte[] paints)
        {
            UsageType = NetUsageType.CHUNKS;
            ID = ServerToClientPacket.BLOCK_EDIT;
            DataStream outp = new DataStream();
            DataWriter dw = new DataWriter(outp);
            dw.WriteInt(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                dw.WriteBytes(pos[i].ToDoubleBytes());
            }
            for (int i = 0; i < mat.Length; i++)
            {
                dw.WriteBytes(Utilities.UshortToBytes(mat[i]));
            }
            dw.WriteBytes(dat);
            dw.WriteBytes(paints);
            dw.Flush();
            Data = outp.ToArray();
        }
    }
}
