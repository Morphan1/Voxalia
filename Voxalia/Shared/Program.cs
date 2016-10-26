//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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

        public const string GameVersion = "0.1.0";

        public const string GlobalServerAddress = "https://frenetic.xyz/";
        
        public static void Init()
        {
            FileHandler files = new FileHandler();
            files.Init();
            MaterialHelpers.Populate(files); // TODO: Non-static material helper data?!
            BlockShapeRegistry.Init();
            FullChunkObject.RegisterMe();
        }
    }
}
