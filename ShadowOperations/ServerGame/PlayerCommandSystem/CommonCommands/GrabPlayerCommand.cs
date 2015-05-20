using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.PlayerCommandSystem.CommonCommands
{
    class GrabPlayerCommand: AbstractPlayerCommand
    {
        public GrabPlayerCommand()
        {
            Name = "grab";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.Player.Grabbed != null)
            {
                entry.Player.Grabbed = null;
                entry.Player.Network.SendMessage("Released."); // TODO: Language // TODO: Noise mode
            }
            else
            {
                Location ang = entry.Player.GetAngles();
                Location end = entry.Player.GetEyePosition() + Utilities.ForwardVector_Deg(ang.Yaw, ang.Pitch) * 2;
                BEPUphysics.Entities.Entity e = entry.Player.TheServer.Collision.CuboidLineTrace(new Location(0.1, 0.1, 0.1), entry.Player.GetEyePosition(), end, entry.Player.IgnoreThis).HitEnt;
                if (e != null && ((PhysicsEntity)e.Tag).GetMass() > 0)
                {
                    entry.Player.Grabbed = (PhysicsEntity)e.Tag;
                    entry.Player.GrabForce = 100f;
                    entry.Player.Network.SendMessage("Grabbed " + entry.Player.Grabbed.EID); // TODO: Language // TODO: Noise mode
                }
                else
                {
                    entry.Player.Network.SendMessage("Grabbed nothing."); // TODO: Language // TODO: Noise mode
                }
            }
        }
    }
}
