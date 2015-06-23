using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class SpawnPointEntity: PrimitiveEntity
    {
        public SpawnPointEntity(World tworld)
            : base(tworld)
        {
            network = false;
            NetworkMe = false;
        }
    }
}
