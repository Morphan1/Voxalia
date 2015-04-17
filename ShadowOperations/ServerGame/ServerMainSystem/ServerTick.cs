using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.ServerMainSystem
{
    public partial class Server
    {
        /// <summary>
        /// How much time has elapsed since the last tick started.
        /// </summary>
        public double Delta;

        /// <summary>
        /// All entities that exist on this server.
        /// </summary>
        public List<Entity> Entities = new List<Entity>();

        /// <summary>
        /// All entities that exist on this server and must tick.
        /// </summary>
        public List<Entity> Tickers = new List<Entity>();

        /// <summary>
        /// All player-type entities that exist on this server.
        /// </summary>
        public List<PlayerEntity> Players = new List<PlayerEntity>();

        /// <summary>
        /// The server's primary tick function.
        /// </summary>
        public void Tick(double delta)
        {
            Delta = delta;
            try
            {
                Networking.Tick();
                ConsoleHandler.CheckInput();
                Commands.Tick();
                TickWorld(Delta);
                for (int i = 0; i < Tickers.Count; i++)
                {
                    Tickers[i].Tick();
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Tick: " + ex.ToString());
            }
        }

        public long cID = 0;

        public void SpawnEntity(Entity e)
        {
            Entities.Add(e);
            e.EID = cID++;
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).SpawnBody();
            }
            if (e is PlayerEntity)
            {
                Players.Add((PlayerEntity)e);
                for (int i = 0; i < Entities.Count - 1; i++)
                {
                    if (e is PhysicsEntity)
                    {
                        ((PlayerEntity)e).Network.SendPacket(new SpawnPhysicsEntityPacketOut((PhysicsEntity)Entities[i]));
                    }
                }
            }
        }
    }
}
