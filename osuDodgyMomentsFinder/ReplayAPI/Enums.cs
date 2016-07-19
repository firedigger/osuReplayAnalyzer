using System;

namespace ReplayAPI
{
    [Flags]
    public enum Keys
    {
        None = 0,
        M1 = (1 << 0),
        M2 = (1 << 1),
        K1 = (1 << 2) | (1 << 0),
        K2 = (1 << 3) | (1 << 1),
    }

    [Flags]
    public enum Mods
    {
        None = 0,
        NoFail = (1 << 0),
        Easy = (1 << 1),
        NoVideo = (1 << 2),
        Hidden = (1 << 3),
        HardRock = (1 << 4),
        SuddenDeath = (1 << 5),
        DoubleTime = (1 << 6),
        Relax = (1 << 7),
        HalfTime = (1 << 8),
        NightCore = (1 << 9),
        FlashLight = (1 << 10),
        Auto = (1 << 11),
        SpunOut = (1 << 12),
        AutoPilot = (1 << 13),
        Perfect = (1 << 14),
        Mania4K = (1 << 15),
        Mania5K = (1 << 16),
        Mania6K = (1 << 17),
        Mania7K = (1 << 18),
        Mania8K = (1 << 19),
    }

    public enum GameModes
    {
        osu = 0,
        Taiko = 1,
        CtB = 2,
        Mania = 3
    }
}