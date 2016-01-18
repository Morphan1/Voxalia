using Voxalia.Shared;
using Voxalia.Shared.Files;
using System.Collections.Generic;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class BlockEditPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            DataStream datums = new DataStream(data);
            DataReader dr = new DataReader(datums);
            int len = dr.ReadInt();
            List<Location> locs = new List<Location>();
            List<Material> mats = new List<Material>();
            for (int i = 0; i < len; i++)
            {
                locs.Add(Location.FromBytes(dr.ReadBytes(12), 0));
            }
            for (int i = 0; i < len; i++)
            {
                mats.Add((Material)Utilities.BytesToUshort(dr.ReadBytes(2)));
            }
            byte[] dats = dr.ReadBytes(len);
            byte[] paints = dr.ReadBytes(len);
            for (int i = 0; i < len; i++)
            {
                TheClient.TheRegion.SetBlockMaterial(locs[i], mats[i], dats[i], paints[i], true); // TODO: Regen in PBAE not SBM.
            }
            return true;
        }
    }
}
