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
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PlaySoundPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 4 + 4 + 24)
            {
                return false;
            }
            string sound = TheClient.Network.Strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4)));
            float vol = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4, 4));
            float pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4 + 4, 4));
            Location pos = Location.FromDoubleBytes(data, 4 + 4 + 4);
            TheClient.Sounds.Play(TheClient.Sounds.GetSound(sound), false, pos, pitch, vol);
            return true;
        }
    }
}
