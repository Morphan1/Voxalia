using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.Shared
{
    public enum PlayerStance : byte
    {
        STAND = 0,
        CROUCH = 1,
        CRAWL = 2
    }

    [Flags]
    public enum YourStatusFlags : byte
    {
        NONE = 0,
        RELOADING = 1,
        NEEDS_RELOAD = 2,
        FOUR = 4,
        EIGHT = 8,
        SIXTEEN = 16,
        THIRTYTWO = 32,
        SIXTYFOUR = 64,
        ONETWENTYEIGHT = 128
    }
}
