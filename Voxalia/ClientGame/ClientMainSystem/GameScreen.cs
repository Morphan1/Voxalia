using Voxalia.ClientGame.UISystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class GameScreen: Screen
    {
        public override void Init()
        {
            // Do nothing, base init currently handles everything for some reason
        }

        public override void Tick()
        {
            // Do nothing, base tick currently handles everything for some reason
        }

        public override void SwitchTo()
        {
            MouseHandler.CaptureMouse();
        }

        public override void Render()
        {
            TheClient.renderGame();
        }
    }
}
