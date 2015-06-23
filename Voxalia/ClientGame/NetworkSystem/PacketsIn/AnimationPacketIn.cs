using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class AnimationPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 4 + 1)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid animation packet length");
                return false;
            }
            long EID = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            string anim = TheClient.Network.Strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, 8, 4)));
            byte mode = data[8 + 4];
            Entity e = TheClient.TheWorld.GetEntity(EID);
            if (e != null && e is EntityAnimated)
            {
                ((EntityAnimated)e).SetAnimation(anim, mode);
                return true;
            }
            SysConsole.Output(OutputType.WARNING, "Not an animated entity: " + EID + " -> " + e);
            return false;
        }
    }
}
