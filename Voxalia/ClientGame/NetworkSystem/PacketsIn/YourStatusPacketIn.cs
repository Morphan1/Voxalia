using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
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
}
