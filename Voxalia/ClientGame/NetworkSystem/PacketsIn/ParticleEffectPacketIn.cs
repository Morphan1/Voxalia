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

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class ParticleEffectPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1 + 4 + 24
                && data.Length != 1 + 4 + 24 + 24)
            {
                return false;
            }
            ParticleEffectNetType type = (ParticleEffectNetType)data[0];
            float fdata1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1, 4));
            Location ldata2 = Location.NaN;
            if (data.Length == 1 + 4 + 24 + 24)
            {
                ldata2 = Location.FromDoubleBytes(data, 1 + 4 + 24);
            }
            Location pos = Location.FromDoubleBytes(data, 1 + 4);
            switch (type)
            {
                case ParticleEffectNetType.EXPLOSION:
                    TheClient.Particles.Explode(pos, fdata1);
                    break;
                case ParticleEffectNetType.SMOKE:
                    TheClient.Particles.Smoke(pos, fdata1, ldata2);
                    break;
                case ParticleEffectNetType.BIG_SMOKE:
                    TheClient.Particles.BigSmoke(pos, fdata1, ldata2);
                    break;
                case ParticleEffectNetType.PAINT_BOMB:
                    TheClient.Particles.PaintBomb(pos, fdata1, ldata2);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
