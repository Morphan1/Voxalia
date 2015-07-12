using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class GameScreen: Screen
    {
        public override void Tick()
        {
            // Do nothing, base tick currently handles everything for some reason
        }

        public override void Render()
        {
            TheClient.renderGame();
        }
    }
}
