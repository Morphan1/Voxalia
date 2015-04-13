using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double Delta;

        public List<Entity> Entities = new List<Entity>();

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            Delta = e.Time;
            try
            {
                TickWorld(Delta);
                // TODO: Tickers
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Ticking: " + ex.ToString());
            }
        }

        /// <summary>
        /// Spawns an entity in the world.
        /// </summary>
        /// <param name="e">The entity to spawn</param>
        public void SpawnEntity(Entity e)
        {
            Entities.Add(e);
            // TODO: Tickers
            e.SpawnBody();
        }
    }
}
