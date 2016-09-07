using System;

namespace Voxalia.Shared
{
    [Flags]
    public enum YourStatusFlags : byte
    {
        NONE = 0,
        RELOADING = 1,
        NEEDS_RELOAD = 2,
        NO_ROTATE = 4,
        INSECURE_MOVEMENT = 8,
        SIXTEEN = 16,
        THIRTYTWO = 32,
        SIXTYFOUR = 64,
        ONETWENTYEIGHT = 128
    }

    public enum BlockDamage : byte
    {
        NONE = 0,
        SOME = 1,
        MUCH = 2,
        FULL = 3
    }

    public enum NetUsageType : byte
    {
        EFFECTS = 0,
        ENTITIES = 1,
        PLAYERS = 2,
        CLOUDS = 3,
        PINGS = 4,
        CHUNKS = 5,
        GENERAL = 6,
        COUNT = 7
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
        MASS = 1,
        HAS_FUEL = 2,
        HELO_TILT_MOD = 3
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
        SMOKE = 1,
        BIG_SMOKE = 2,
        PAINT_BOMB = 3
    }

    public enum ModelCollisionMode : byte
    {
        PRECISE = 1,
        AABB = 2,
        SPHERE = 3,
        CONVEXHULL = 4
        // TODO: ConvexHull!
    }

    public enum ClientStatus : byte
    {
        TYPING = 0,
        AFK = 1
    }

    public enum BeamType : byte
    {
        STRAIGHT = 0,
        CURVE = 1,
        MULTICURVE = 2
    }
    
    public enum KeysPacketData : ushort
    {
        UPWARD = 1,
        CLICK = 2,
        ALTCLICK = 4,
        DOWNWARD = 8,
        USE = 16,
        ITEMLEFT = 32,
        ITEMRIGHT = 64,
        ITEMUP = 128,
        ITEMDOWN = 256
    }
    
    public enum ChunkReachability : byte
    {
        ZP_ZM = 0,
        ZP_XP = 1,
        ZP_YP = 2,
        ZP_XM = 3,
        ZP_YM = 4,
        ZM_XP = 5,
        ZM_YP = 6,
        ZM_XM = 7,
        ZM_YM = 8,
        XP_YP = 9,
        XP_YM = 10,
        XP_XM = 11,
        XM_YP = 12,
        XM_YM = 13,
        YP_YM = 14,
        COUNT = 15
    }

    public enum ClientToServerPacket : byte
    {
        PING = 0,
        KEYS = 1,
        COMMAND = 2,
        HOLD_ITEM = 3,
        DISCONNECT = 4,
        SET_STATUS = 5,
        PLEASE_REDEFINE = 6
    }

    public enum ServerToClientPacket : byte
    {
        PING = 0,
        YOUR_POSITION = 1,
        SPAWN_ENTITY = 2,
        PHYSICS_ENTITY_UPDATE = 3,
        MESSAGE = 4,
        CHARACTER_UPDATE = 5,
        // ...
        DESPAWN_ENTITY = 7,
        NET_STRING = 8,
        SPAWN_ITEM = 9,
        YOUR_STATUS = 10,
        ADD_JOINT = 11,
        YOUR_EID = 12,
        DESTROY_JOINT = 13,
        // ...
        PRIMITIVE_ENTITY_UPDATE = 15,
        ANIMATION = 16,
        FLASHLIGHT = 17,
        REMOVE_ITEM = 18,
        SET_ITEM = 19,
        CVAR_SET = 20,
        SET_HELD_ITEM = 21,
        CHUNK_INFO = 22,
        BLOCK_EDIT = 23,
        SUN_ANGLE = 24,
        TELEPORT = 25,
        OPERATION_STATUS = 26,
        PARTICLE_EFFECT = 27,
        PATH = 28,
        CHUNK_FORGET = 29,
        FLAG_ENTITY = 30,
        DEFAULT_SOUND = 31,
        GAIN_CONTROL_OF_VEHICLE = 32,
        ADD_CLOUD = 33,
        REMOVE_CLOUD = 34,
        ADD_TO_CLOUD = 35,
        // ...
        SET_STATUS = 37,
        HIGHLIGHT = 38,
        PLAY_SOUND = 39,
        LOD_MODEL = 40,
        LOSE_CONTROL_OF_VEHICLE = 41
    }

    public enum NetworkEntityType : byte
    {
        NONE = 0,
        BULLET = 1,
        PRIMITIVE = 2,
        CHARACTER = 3,
        GLOWSTICK = 4,
        GRENADE = 5,
        BLOCK_GROUP = 6,
        BLOCK_ITEM = 7,
        STATIC_BLOCK = 8,
        MODEL = 9
    }

    public enum TextChannel : byte
    {
        UNSPECIFIED = 0,
        CHAT = 1,
        BROADCAST = 2,
        COMMAND_RESPONSE = 3,
        DEBUG_INFO = 4,
        IMPORTANT = 5,
        COUNT = 6
    }
}
