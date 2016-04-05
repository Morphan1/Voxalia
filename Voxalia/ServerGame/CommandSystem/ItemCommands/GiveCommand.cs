using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.TagSystem.TagObjects;
using FreneticScript.TagHandlers;

namespace Voxalia.ServerGame.CommandSystem.ItemCommands
{
    public class GiveCommand : AbstractCommand
    {
        public Server TheServer;

        public GiveCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "give";
            Description = "Gives an item to a player.";
            Arguments = "<players> <items>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(queue, entry);
                return;
            }
            ListTag players = ListTag.For(entry.GetArgumentObject(queue, 0));
            ListTag items = ListTag.For(entry.GetArgumentObject(queue, 1));
            List<ItemStack> itemlist = new List<ItemStack>();
            for (int i = 0; i < items.ListEntries.Count; i++)
            {
                ItemTag item = ItemTag.For(TheServer, items.ListEntries[i]);
                if (item == null)
                {
                    queue.HandleError(entry, "Invalid item!");
                    return;
                }
                itemlist.Add(item.Internal);
            }
            List<PlayerEntity> playerlist = new List<PlayerEntity>();
            for (int i = 0; i < players.ListEntries.Count; i++)
            {
                PlayerTag player = PlayerTag.For(TheServer, players.ListEntries[i]);
                if (player == null)
                {
                    queue.HandleError(entry, "Invalid player: " + TagParser.Escape(items.ListEntries[i].ToString()));
                    return;
                }
                playerlist.Add(player.Internal);
            }
            foreach (PlayerEntity player in playerlist)
            {
                foreach (ItemStack item in itemlist)
                {
                    player.Items.GiveItem(item);
                }
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, itemlist.Count + " item(s) given to " + playerlist.Count + " player(s)!");
            }
        }
    }
}
