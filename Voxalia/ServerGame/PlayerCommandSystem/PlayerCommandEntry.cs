using System.Collections.Generic;
using System.Text;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.PlayerCommandSystem
{
    public class PlayerCommandEntry
    {
        public PlayerEntity Player;

        public AbstractPlayerCommand Command;

        public List<string> InputArguments;

        public string AllArguments()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < InputArguments.Count; i++)
            {
                sb.Append(InputArguments[i]);
                if (i + 1 < InputArguments.Count)
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
    }
}
