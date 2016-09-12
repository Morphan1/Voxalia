using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class AddCloudPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            Location pos = Location.FromDoubleBytes(dr.ReadBytes(24), 0);
            Location vel = Location.FromDoubleBytes(dr.ReadBytes(24), 0);
            long cid = dr.ReadLong();
            for (int i = 0; i < TheClient.TheRegion.Clouds.Count; i++)
            {
                if (TheClient.TheRegion.Clouds[i].CID == cid)
                {
                    TheClient.TheRegion.Clouds.RemoveAt(i);
                    break;
                }
            }
            Cloud cloud = new Cloud(TheClient.TheRegion, pos);
            cloud.Velocity = vel;
            cloud.CID = cid;
            int count = dr.ReadInt();
            for (int i = 0; i < count; i++)
            {
                cloud.Points.Add(Location.FromDoubleBytes(dr.ReadBytes(24), 0));
                cloud.Sizes.Add(dr.ReadFloat());
                cloud.EndSizes.Add(dr.ReadFloat());
            }
            TheClient.TheRegion.Clouds.Add(cloud);
            dr.Close();
            return true;
        }
    }
}
