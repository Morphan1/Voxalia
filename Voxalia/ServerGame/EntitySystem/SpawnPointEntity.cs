using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
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
