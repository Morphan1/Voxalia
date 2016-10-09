//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using Voxalia.Shared.Files;
using System.Collections.Generic;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class BlockEditPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            DataStream datums = new DataStream(data);
            DataReader dr = new DataReader(datums);
            int len = dr.ReadInt();
            List<Location> locs = new List<Location>();
            List<ushort> mats = new List<ushort>();
            for (int i = 0; i < len; i++)
            {
                locs.Add(Location.FromDoubleBytes(dr.ReadBytes(24), 0));
            }
            for (int i = 0; i < len; i++)
            {
                mats.Add(Utilities.BytesToUshort(dr.ReadBytes(2)));
            }
            byte[] dats = dr.ReadBytes(len);
            byte[] paints = dr.ReadBytes(len);
            for (int i = 0; i < len; i++)
            {
                TheClient.TheRegion.SetBlockMaterial(locs[i], mats[i], dats[i], paints[i], true); // TODO: Regen in PBAE not SBM.
            }
            return true;
        }
    }
}
