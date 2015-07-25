namespace Voxalia.ClientGame.ClientMainSystem
{
    public abstract class Screen
    {
        public Client TheClient;

        public abstract void Tick();

        public abstract void Render();

        public abstract void Init();

        public abstract void SwitchTo();
    }
}
