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
        public static string GameName;

        public static string GameVersion;

        public static FileHandler Files;

        public static void Init()
        {
            FullChunkObject.RegisterMe();
        }
    }
}
