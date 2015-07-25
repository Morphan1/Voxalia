namespace Voxalia.ServerGame.EntitySystem
{
    public interface EntityTargettable
    {
        string GetTargetName();

        void Trigger(Entity ent, Entity user);
    }
}
