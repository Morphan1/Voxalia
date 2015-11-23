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
    }
}
