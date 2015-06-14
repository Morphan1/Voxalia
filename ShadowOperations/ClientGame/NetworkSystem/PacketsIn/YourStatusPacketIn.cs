using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class YourStatusPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 4 + 1)
            {
                return false;
            }
            float health = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            float maxhealth = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4, 4));
            TheClient.Player.Health = health;
            TheClient.Player.MaxHealth = maxhealth;
            TheClient.Player.ServerFlags = (YourStatusFlags)data[4 + 4];
            return true;
        }
    }

    [Flags]
    public enum YourStatusFlags : byte
    {
        NONE = 0,
        RELOADING = 1,
        NEEDS_RELOAD = 2,
        FOUR = 4,
        EIGHT = 8,
        SIXTEEN = 16,
        THIRTYTWO = 32,
        SIXTYFOUR = 64,
        ONETWENTYEIGHT = 128
    }
}
