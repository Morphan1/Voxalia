using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class SpawnPointEntity: PrimitiveEntity
    {
        public SpawnPointEntity(Server tserver)
            : base(tserver)
        {
            network = false;
            NetworkMe = false;
        }
    }
}
