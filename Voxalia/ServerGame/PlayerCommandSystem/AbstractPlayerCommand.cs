namespace Voxalia.ServerGame.PlayerCommandSystem
{
    public abstract class AbstractPlayerCommand
    {
        public string Name = null;

        public bool Silent = false;

        public abstract void Execute(PlayerCommandEntry entry);
    }
}
