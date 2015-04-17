using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class CubeEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(1);

        public string Textures = "top|bottom|xp|xm|yp|ym";
        public string TexCoords = "1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f|1/1/0/0/f/f";

        public CubeEntity(Location half, Server tserver, float mass)
            : base(tserver, true)
        {
            HalfSize = half;
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            SetMass(mass);
        }

        bool pActive = false;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                PhysicsEntityUpdatePacketOut peupo = new PhysicsEntityUpdatePacketOut(this);
                for (int i = 0; i < TheServer.Players.Count; i++)
                {
                    TheServer.Players[i].Network.SendPacket(peupo);
                }
            }
        }
    }
}
