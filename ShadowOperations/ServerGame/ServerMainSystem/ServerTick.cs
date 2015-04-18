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
            if (e.IsSpawned)
            {
                return;
            }
            Entities.Add(e);
            e.IsSpawned = true;
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
            // TODO: Send spawn packet to all players
        }

        public void DespawnEntity(Entity e)
        {
            if (!e.IsSpawned)
            {
                return;
            }
            Entities.Remove(e);
            e.IsSpawned = false;
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).DestroyBody();
            }
            if (e is PlayerEntity)
            {
                Players.Remove((PlayerEntity)e);
                //((PlayerEntity)e).Kick("Despawned.");
            }
            // TODO: Send despawn packet to all players
        }

        public void LoadMapFromString(string data)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (!(Entities[i] is PlayerEntity))
                {
                    DespawnEntity(Entities[i]);
                    i--;
                }
            }
            data = data.Replace('\r', '\n').Replace('\t', '\n').Replace("\n", "");
            int start = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '{')
                {
                    string objo = data.Substring(start, i - start).Trim();
                    int obj_start = i + 1;
                    for (int x = i + 1; x < data.Length; x++)
                    {
                        if (data[x] == '}')
                        {
                            try
                            {
                                string objdat = data.Substring(obj_start, x - obj_start);
                                LoadObj(objo, objdat);
                            }
                            catch (Exception ex)
                            {
                                SysConsole.Output(OutputType.ERROR, "Invalid entity " + objo + ": " + ex.ToString());
                            }
                            i = x;
                            break;
                        }
                    }
                    start = i + 1;
                }
            }
            // TODO: Respawn all players
        }

        public void LoadObj(string name, string dat)
        {
            string[] dats = dat.Split(';');
            if (name == "general")
            {
                for (int i = 0; i < dats.Length; i++)
                {
                    if (dats[i].Length <= 0)
                    {
                        continue;
                    }
                    string trimmed = dats[i].Trim();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }
                    string[] datum = trimmed.Split(':');
                    if (datum.Length != 2)
                    {
                        throw new Exception("Invalid key '" + dats[i] + "'!");
                    }
                    switch (datum[0])
                    {
                        case "ambient":
                            // TODO: ambient = Utilities.StringToLocation(datum[1]);
                            break;
                        case "music":
                            // TODO: music = datum[1];
                            break;
                        default:
                            throw new Exception("Invalid key: " + datum[0].Trim() + "!");
                    }
                }
                return;
            }
            Entity e;
            switch (name) // TODO: Registry
            {
                case "cube":
                    e = new CubeEntity(new Location(1, 1, 1), this, 0f);
                    break;
                case "point_light":
                    // TODO: e = new PointLightEntity(new Location(0), 1, new Location(1), false);
                    return;
                    break;
                case "spawn":
                    // TODO: e = new SpawnPointEntity(new Location(0));
                    return;
                    break;
                default:
                    throw new Exception("Invalid entity type '" + name + "'!");
            }
            for (int i = 0; i < dats.Length; i++)
            {
                if (dats[i].Length <= 0)
                {
                    continue;
                }
                string trimmed = dats[i].Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                string[] datum = trimmed.Split(':');
                if (datum.Length != 2)
                {
                    throw new Exception("Invalid key '" + dats[i] + "'!");
                }
                if (!e.ApplyVar(datum[0].Trim(), datum[1].Trim()))
                {
                    throw new Exception("Invalid key: " + datum[0].Trim() + "!");
                }
            }
            e.Recalculate();
            SpawnEntity(e);
        }
    }
}
