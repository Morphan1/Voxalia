using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double Delta;

        public List<Entity> Entities = new List<Entity>();

        public List<Entity> Tickers = new List<Entity>();

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            Delta = e.Time;
            try
            {
                KeyHandler.Tick();
                MouseHandler.Tick();
                UIConsole.Tick();
                Commands.Tick();
                Network.Tick();
                TickWorld(Delta);
                for (int i = 0; i < Tickers.Count; i++)
                {
                    Tickers[i].Tick();
                }
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
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).SpawnBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Spawn();
            }
        }

        public void Despawn(Entity e)
        {
            Entities.Remove(e);
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).DestroyBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
        }

        public Entity GetEntity(long EID)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].EID == EID)
                {
                    return Entities[i];
                }
            }
            return null;
        }
    }
}
