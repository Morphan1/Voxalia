using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;

namespace Voxalia.Shared
{
    public class Program
    {
        public const string GameName = "Voxalia";

        public const string GameVersion = "0.0.9";

        public const string GlobalServerAddress = "https://frenetic.xyz/";

        public static FileHandler Files;

        public static void Init()
        {
            MaterialHelpers.Populate(Files);
            FullChunkObject.RegisterMe();
        }
    }
}
