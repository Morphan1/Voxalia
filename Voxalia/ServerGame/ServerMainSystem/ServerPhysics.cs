using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    public partial class Server
    {
        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
        }

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
