using System;

namespace Voxalia.Shared
{
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

    public enum StatusOperation : byte
    {
        NONE = 0,
        CHUNK_LOAD = 1
    }

    public enum BGETraceMode : byte
    {
        CONVEX = 0,
        PERFECT = 1
    }

    public enum EntityFlag : byte
    {
        FLYING = 0,
        MASS = 1
    }

    public enum DefaultSound : byte
    {
        STEP = 0,
        PLACE = 1,
        BREAK = 2
    }

    public enum ParticleEffectNetType : byte
    {
        EXPLOSION = 0,
        SMOKE = 1
    }
}
