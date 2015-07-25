using Voxalia.ServerGame.ItemSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnItemPacketOut: AbstractPacketOut
    {
        public SpawnItemPacketOut(int spot, ItemStack item)
        {
            ID = 10;
            byte[] itemdat = item.ToBytes();
            Data = new byte[4 + itemdat.Length];
            Utilities.IntToBytes(spot).CopyTo(Data, 0);
            itemdat.CopyTo(Data, 4);
        }
    }
}
