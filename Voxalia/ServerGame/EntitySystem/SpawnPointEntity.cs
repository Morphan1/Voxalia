using System;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    // TODO: Remove!
    public class SpawnPointEntity: PrimitiveEntity
    {
        public SpawnPointEntity(Region tregion)
            : base(tregion)
        {
            network = false;
            NetworkMe = false;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.LEGACY_SPAWNPOINT;
        }

        public override byte[] GetSaveBytes()
        {
            // Does not save!
            return null;
        }
    }
}
