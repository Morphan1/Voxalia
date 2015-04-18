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
                BEPUphysics.Entities.Entity e = entry.Player.TheServer.Collision.CuboidLineEntity(new Location(0.1, 0.1, 0.1),
                    entry.Player.GetPosition(), entry.Player.GetPosition() + Utilities.ForwardVector_Deg(ang.X, ang.Y) * 2);
                if (e != null)
                {
                    entry.Player.Grabbed = (PhysicsEntity)e.Tag;
                    entry.Player.Network.SendMessage("Grabbed."); // TODO: Language // TODO: Noise mode
                }
                else
                {
                    entry.Player.Network.SendMessage("Grabbed nothing."); // TODO: Language // TODO: Noise mode
                }
            }

        }
    }
}
