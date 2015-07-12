using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public abstract class Screen
    {
        public Client TheClient;

        public abstract void Tick();

        public abstract void Render();
    }
}
