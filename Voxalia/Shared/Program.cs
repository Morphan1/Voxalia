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
        public static string GameName = "Voxalia";

        public static string GameVersion = "0.0.9";

        public static FileHandler Files;

        public static void Init()
        {
            MaterialHelpers.Populate(Files);
            FullChunkObject.RegisterMe();
        }
    }
}
