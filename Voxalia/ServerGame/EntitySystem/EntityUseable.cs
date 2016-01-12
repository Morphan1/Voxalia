namespace Voxalia.ServerGame.EntitySystem
{
    public interface EntityUseable
    {
        void StartUse(Entity user);

        void StopUse(Entity user);
    }
}
