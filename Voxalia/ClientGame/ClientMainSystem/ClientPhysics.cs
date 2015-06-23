using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        // Note: the client only has one world loaded at any given time.
        public World TheWorld = null;

        public void BuildWorld()
        {
            TheWorld = new World();
            TheWorld.BuildWorld();
            TheWorld.TheClient = this;
        }

        public void TickWorld(double delta)
        {
            TheWorld.TickWorld(delta);
        }
    }
}
