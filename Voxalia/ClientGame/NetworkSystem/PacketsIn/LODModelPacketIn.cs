using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class LODModelPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 4 + 16 + 8 + 12)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            int modInd = Utilities.BytesToInt(Utilities.BytesPartial(data, 12, 4));
            string modname = TheClient.Network.Strings.StringForIndex(modInd);
            PrimitiveModelEntity pme = new PrimitiveModelEntity(modname, TheClient.TheRegion);
            pme.SetPosition(pos);
            float qX = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 4, 4));
            float qY = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 4 + 4, 4));
            float qZ = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 4 + 8, 4));
            float qW = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 4 + 12, 4));
            pme.SetOrientation(new BEPUutilities.Quaternion(qX, qY, qZ, qW));
            pme.EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 4 + 16, 8));
            pme.scale = Location.FromBytes(data, 12 + 4 + 16 + 8);
            return true;
        }
    }
}
