using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class MainMenuScreen: Screen
    {
        public override void Tick()
        {
        }

        public override void Render()
        {
            TheClient.Establish2D();
        }
    }
}
