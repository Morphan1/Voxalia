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

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ParticleEffectPacketOut: AbstractPacketOut
    {
        public ParticleEffectPacketOut(ParticleEffectNetType type, double dat1, Location pos)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.PARTICLE_EFFECT;
            Data = new byte[1 + 4 + 24];
            Data[0] = (byte)type;
            Utilities.FloatToBytes((float)dat1).CopyTo(Data, 1);
            pos.ToDoubleBytes().CopyTo(Data, 1 + 4);
        }

        public ParticleEffectPacketOut(ParticleEffectNetType type, double dat1, Location pos, Location dat2)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.PARTICLE_EFFECT;
            Data = new byte[1 + 4 + 24 + 24];
            Data[0] = (byte)type;
            Utilities.FloatToBytes((float)dat1).CopyTo(Data, 1);
            pos.ToDoubleBytes().CopyTo(Data, 1 + 4);
            dat2.ToDoubleBytes().CopyTo(Data, 1 + 4 + 24);
        }
    }
}
