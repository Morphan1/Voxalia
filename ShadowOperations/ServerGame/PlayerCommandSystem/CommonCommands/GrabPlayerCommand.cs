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
                entry.Player.TheServer.PhysicsWorld.Remove(entry.Player.Body);
                BEPUphysics.Entities.Entity e = entry.Player.TheServer.Collision.CuboidLineEntity(new Location(0.1, 0.1, 0.1),
                    entry.Player.GetEyePosition(), entry.Player.GetEyePosition() + Utilities.ForwardVector_Deg(ang.X, ang.Y) * 5);
                entry.Player.TheServer.PhysicsWorld.Add(entry.Player.Body);
                SysConsole.Output(OutputType.INFO, e == null ? "null": e.Tag.ToString());
                if (e != null && ((PhysicsEntity)e.Tag).GetMass() > 0)
                {
                    entry.Player.Grabbed = (PhysicsEntity)e.Tag;
                    entry.Player.Network.SendMessage("Grabbed " + entry.Player.Grabbed); // TODO: Language // TODO: Noise mode
                }
                else
                {
                    entry.Player.Network.SendMessage("Grabbed nothing."); // TODO: Language // TODO: Noise mode
                }
            }

        }
    }
}
