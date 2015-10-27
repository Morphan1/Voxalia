using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    // TODO: Rename or scrap file?
    public partial class Server
    {
        public List<Region> LoadedWorlds = new List<Region>();

        public void LoadWorld(string name)
        {
            // TODO: Actually load from file!
            Region world = new Region();
            world.Name = name.ToLower();
            world.TheServer = this;
            world.BuildWorld();
            LoadedWorlds.Add(world);
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorlds(double delta)
        {
            for (int i = 0; i < LoadedWorlds.Count; i++)
            {
                LoadedWorlds[i].Tick(delta);
            }
        }
    }
}
