using FreneticScript;
using FreneticScript.CommandSystem;
using System;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A command to quickly switch and use a specific item.
    /// </summary>
    class QuickItemCommand : AbstractCommand
    {
        public Client TheClient;

        public QuickItemCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "quickitem";
            Description = "Switches to and uses an item by the given number.";
            Arguments = "'hold'/'throw'/'click'/'alt'/'drop' <slot number>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Marker == 0 || entry.Marker == 3)
            {
                queue.HandleError(entry, "Must use + or -");
            }
            else if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            else if (entry.Marker == 1)
            {
                if (entry.Arguments.Count < 1)
                {
                    entry.Bad(queue, "Must specify a use type and a slot number!");
                    return;
                }
                if (TheClient.PrevQuickItem != -1)
                {
                    return;
                }
                string useType = entry.GetArgument(queue, 0).ToLowerFast();
                if (useType != "hold" && useType != "throw" && useType != "click" && useType != "alt" && useType != "drop")
                {
                    entry.Bad(queue, "Invalid use type!");
                    return;
                }
                TheClient.PrevQuickItem = TheClient.QuickBarPos;
                TheClient.QuickItemUseType = useType;
                int slot = Math.Abs(Utilities.StringToInt(entry.GetArgument(queue, 1))) % (TheClient.Items.Count + 1);
                TheClient.SetHeldItemSlot(slot);
                switch (useType)
                {
                    case "hold":
                        break;
                    case "throw":
                        break;
                    case "click":
                        TheClient.Player.Click = true;
                        break;
                    case "alt":
                        TheClient.Player.AltClick = true;
                        break;
                    case "drop":
                        break;
                }
            }
            else if (entry.Marker == 2)
            {
                if (TheClient.PrevQuickItem == -1)
                {
                    return;
                }
                TheClient.SetHeldItemSlot(TheClient.PrevQuickItem);
                TheClient.PrevQuickItem = -1;
                switch (TheClient.QuickItemUseType)
                {
                    case "hold":
                        break;
                    case "throw":
                        break;
                    case "click":
                        TheClient.Player.Click = false;
                        break;
                    case "alt":
                        TheClient.Player.AltClick = false;
                        break;
                    case "drop":
                        break;
                }
            }
        }
    }
}
