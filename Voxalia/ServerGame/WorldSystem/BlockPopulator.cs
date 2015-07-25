namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BlockPopulator
    {
        public abstract float GetHeight(short seed, short seed2, float x, float y);

        public abstract void Populate(short seed, short seed2, Chunk chunk);
    }
}
