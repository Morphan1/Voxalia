using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class PlayerEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(0.3f, 0.3f, 1f);

        public Connection Network;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public PlayerEntity(Server tserver, Connection conn)
            : base(tserver, true)
        {
            Network = conn;
        }
    }
}
