using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
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
