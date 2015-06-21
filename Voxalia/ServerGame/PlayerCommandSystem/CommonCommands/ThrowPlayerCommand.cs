using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.ItemSystem;

namespace ShadowOperations.ServerGame.PlayerCommandSystem.CommonCommands
{
    class ThrowPlayerCommand : AbstractPlayerCommand
    {
        public ThrowPlayerCommand()
        {
            Name = "throw";
            Silent = true;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            ItemStack stack = entry.Player.GetItemForSlot(entry.Player.cItem);
            if (stack.IsBound)
            {
                if (stack.Info == entry.Player.TheServer.Items.GetInfoFor("open_hand"))
                {
                    if (entry.Player.GrabJoint != null)
                    {
                        BEPUutilities.Vector3 launchvec = (entry.Player.ForwardVector() * 100).ToBVector(); // TODO: Strength limits
                        PhysicsEntity pe = entry.Player.GrabJoint.Ent2;
                        entry.Player.TheServer.DestroyJoint(entry.Player.GrabJoint);
                        entry.Player.GrabJoint = null;
                        pe.Body.ApplyLinearImpulse(ref launchvec);
                        pe.Body.ActivityInformation.Activate();
                        return;
                    }
                }
                entry.Player.Network.SendMessage("^1Can't throw this."); // TODO: Language, entry.output, etc.
                return;
            }
            ItemEntity ie = new ItemEntity(stack, entry.Player.TheServer);
            // TODO: Animate player
            Location fvel = entry.Player.ForwardVector();
            ie.SetPosition(entry.Player.GetEyePosition() + fvel);
            ie.SetOrientation(entry.Player.GetOrientation());
            ie.SetVelocity(fvel * 10);
            entry.Player.TheServer.SpawnEntity(ie);
            entry.Player.RemoveItem(entry.Player.cItem);
        }
    }
}
