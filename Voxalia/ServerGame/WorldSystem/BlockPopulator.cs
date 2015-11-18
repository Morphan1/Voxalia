namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BlockPopulator
    {
        public abstract float GetHeight(int seed, int seed2, float x, float y);

        public abstract void Populate(int seed, int seed2, int seed3, int seed4, int seed5, Chunk chunk);
    }
}
