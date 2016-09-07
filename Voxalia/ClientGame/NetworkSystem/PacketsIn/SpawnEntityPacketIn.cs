using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnEntityPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 1 + 8)
            {
                return false;
            }
            NetworkEntityType etype = (NetworkEntityType)data[0];
            long eid = Utilities.BytesToLong(Utilities.BytesPartial(data, 1, 8));
            byte[] rem = new byte[data.Length - (8 + 1)];
            Array.Copy(data, 8 + 1, rem, 0, data.Length - (8 + 1));
            EntityTypeConstructor etc;
            if (TheClient.EntityConstructors.TryGetValue(etype, out etc))
            {
                Entity e = etc.Create(TheClient.TheRegion, rem);
                if (e == null)
                {
                    return false;
                }
                e.EID = eid;
                TheClient.TheRegion.SpawnEntity(e);
                if (e is PhysicsEntity && (e as PhysicsEntity).GenBlockShadows)
                {
                    Chunk ch = TheClient.TheRegion.GetChunk(TheClient.TheRegion.ChunkLocFor(e.GetPosition()));
                    if (ch != null)
                    {
                        ch.CreateVBO();
                    }
                }
                return true;
            }
            return false;
        }
    }
}
