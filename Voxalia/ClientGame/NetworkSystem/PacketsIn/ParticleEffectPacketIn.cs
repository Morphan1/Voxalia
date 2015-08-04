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
            if (data.Length != 1 + 4 + 12)
            {
                return false;
            }
            byte type = data[0];
            float fdata1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1, 4));
            Location pos = Location.FromBytes(data, 1 + 4);
            switch (type)
            {
                case 0:
                    TheClient.Particles.Explode(pos, fdata1);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
