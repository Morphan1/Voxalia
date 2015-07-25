using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;

namespace Voxalia.ClientGame.CommandSystem
{
    class ClientOutputter : Outputter
    {
        public Client TheClient;

        public ClientOutputter(Client tclient)
        {
            TheClient = tclient;
        }

        public override void WriteLine(string text)
        {
            UIConsole.WriteLine(text);
        }

        public override void Good(string tagged_text, DebugMode mode)
        {
            string text = TheClient.Commands.CommandSystem.TagSystem.ParseTags(tagged_text, TextStyle.Color_Outgood, null, mode);
            UIConsole.WriteLine(TextStyle.Color_Outgood + text);
        }

        public override void Bad(string tagged_text, DebugMode mode)
        {
            string text = TheClient.Commands.CommandSystem.TagSystem.ParseTags(tagged_text, TextStyle.Color_Outbad, null, mode);
            UIConsole.WriteLine(TextStyle.Color_Outbad + text);
        }

        public override void UnknownCommand(CommandQueue queue, string basecommand, string[] arguments)
        {
            if (TheClient.Network.IsAlive)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(basecommand);
                for (int i = 0; i < arguments.Length; i++)
                {
                    sb.Append("\n").Append(queue.ParseTags ? TheClient.Commands.CommandSystem.TagSystem.ParseTags(arguments[i], TextStyle.Color_Simple, null, DebugMode.MINIMAL): arguments[i]);
                }
                CommandPacketOut packet = new CommandPacketOut(sb.ToString());
                TheClient.Network.SendPacket(packet);
            }
            else
            {
                WriteLine(TextStyle.Color_Error + "Unknown command '" +
                    TextStyle.Color_Standout + basecommand + TextStyle.Color_Error + "'.");
            }
        }

        public override string ReadTextFile(string name)
        {
            return Program.Files.ReadText("scripts/" + name);
        }
    }
}
