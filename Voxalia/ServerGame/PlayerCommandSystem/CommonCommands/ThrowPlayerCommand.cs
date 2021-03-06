//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
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
            ItemStack stack = entry.Player.Items.GetItemForSlot(entry.Player.Items.cItem);
            if (stack.IsBound)
            {
                if (stack.Info.Name == "open_hand") // TODO: Better handling of special cases -> Info.Throw() ?
                {
                    if (entry.Player.GrabJoint != null)
                    {
                        BEPUutilities.Vector3 launchvec = (entry.Player.ForwardVector() * 100).ToBVector(); // TODO: Strength limits
                        PhysicsEntity pe = entry.Player.GrabJoint.Ent2;
                        entry.Player.TheRegion.DestroyJoint(entry.Player.GrabJoint);
                        entry.Player.GrabJoint = null;
                        pe.Body.ApplyLinearImpulse(ref launchvec);
                        pe.Body.ActivityInformation.Activate();
                        return;
                    }
                }
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "^1Can't throw this."); // TODO: Language, entry.output, etc.
                return;
            }
            ItemStack item = stack.Duplicate();
            item.Count = 1;
            PhysicsEntity ie = entry.Player.TheRegion.ItemToEntity(item);
            // TODO: Animate player
            Location fvel = entry.Player.ForwardVector();
            ie.SetPosition(entry.Player.GetEyePosition() + fvel * 2);
            ie.SetOrientation(entry.Player.GetOrientation());
            ie.SetVelocity(fvel * 15);
            entry.Player.TheRegion.SpawnEntity(ie);
            if (stack.Count > 1)
            {
                stack.Count -= 1;
                entry.Player.Network.SendPacket(new SetItemPacketOut(entry.Player.Items.cItem - 1, stack));
            }
            else
            {
                entry.Player.Items.RemoveItem(entry.Player.Items.cItem);
            }
        }
    }
}
