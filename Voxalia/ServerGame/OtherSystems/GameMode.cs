using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.OtherSystems
{
    public enum GameMode
    {
        /// <summary>
        /// No block modification, only picked up items, no flying (excluding flight items).
        /// </summary>
        EXPLORER = 1,

        /// <summary>
        /// Block modification (restricted: slow breaking, requires tools, etc.), only picked up items, no flying (excluding flight items).
        /// </summary>
        SURVIVOR = 2,

        /// <summary>
        /// Block modification (restricted: only what you can do with the items you have, placed one by one), any items, flying.
        /// </summary>
        SIMPLE_BUILDER = 3,

        /// <summary>
        /// Block modification (full high-powered), any tems, flying.
        /// </summary>
        BUILDER = 4,

        /// <summary>
        /// No block modification, no items, flying.
        /// </summary>
        SPECTATOR = 5
    }
}
