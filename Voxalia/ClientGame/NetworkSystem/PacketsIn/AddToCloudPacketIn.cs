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
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class AddToCloudPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 24 + 4 + 4 + 8)
            {
                return false;
            }
            Location loc = Location.FromDoubleBytes(data, 0);
            float size = Utilities.BytesToFloat(Utilities.BytesPartial(data, 24, 4));
            float endsize = Utilities.BytesToFloat(Utilities.BytesPartial(data, 24 + 4, 4));
            long CID = Utilities.BytesToLong(Utilities.BytesPartial(data, 24 + 4 + 4, 8));
            for (int i = 0; i < TheClient.TheRegion.Clouds.Count; i++)
            {
                if (TheClient.TheRegion.Clouds[i].CID == CID)
                {
                    TheClient.TheRegion.Clouds[i].Points.Add(loc);
                    TheClient.TheRegion.Clouds[i].Sizes.Add(size);
                    TheClient.TheRegion.Clouds[i].EndSizes.Add(endsize);
                    return true;
                }
            }
            return false;
        }
    }
}
