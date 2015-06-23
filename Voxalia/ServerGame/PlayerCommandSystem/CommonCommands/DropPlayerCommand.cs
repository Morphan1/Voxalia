using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.ItemSystem;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class DropPlayerCommand : AbstractPlayerCommand
    {
        public DropPlayerCommand()
        {
            Name = "drop";
            Silent = true;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            int it = entry.Player.cItem;
            if (entry.InputArguments.Count > 0)
            {
                it = Utilities.StringToInt(entry.InputArguments[0]);
            }
            ItemStack stack = entry.Player.GetItemForSlot(it);
            if (stack.IsBound)
            {
                if (stack.Info == entry.Player.TheServer.Items.GetInfoFor("open_hand")
                    && entry.Player.GrabJoint != null)
                {
                    entry.Player.TheWorld.DestroyJoint(entry.Player.GrabJoint);
                    entry.Player.GrabJoint = null;
                    return;
                }
                entry.Player.Network.SendMessage("^1Can't drop this."); // TODO: Language, entry.output, etc.
                return;
            }
            ItemEntity ie = new ItemEntity(stack, entry.Player.TheWorld);
            // TODO: Animate player
            Location fvel = entry.Player.ForwardVector();
            ie.SetPosition(entry.Player.GetEyePosition() + fvel);
            ie.SetOrientation(entry.Player.GetOrientation());
            ie.SetVelocity(fvel);
            entry.Player.TheWorld.SpawnEntity(ie);
            entry.Player.RemoveItem(entry.Player.cItem);
        }
    }
}
