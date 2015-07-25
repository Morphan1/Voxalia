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
