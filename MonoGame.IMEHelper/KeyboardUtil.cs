﻿using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

namespace MonoGame.IMEHelper;

public static class KeyboardUtil
{
    private static readonly Dictionary<int, Keys> Map = new()
    {
        [8] = Keys.Back,
        [9] = Keys.Tab,
        [13] = Keys.Enter,
        [27] = Keys.Escape,
        [32] = Keys.Space,
        [39] = Keys.OemQuotes,
        [43] = Keys.Add,
        [44] = Keys.OemComma,
        [45] = Keys.OemMinus,
        [46] = Keys.OemPeriod,
        [47] = Keys.OemQuestion,
        [48] = Keys.D0,
        [49] = Keys.D1,
        [50] = Keys.D2,
        [51] = Keys.D3,
        [52] = Keys.D4,
        [53] = Keys.D5,
        [54] = Keys.D6,
        [55] = Keys.D7,
        [56] = Keys.D8,
        [57] = Keys.D9,
        [59] = Keys.OemSemicolon,
        [60] = Keys.OemBackslash,
        [61] = Keys.OemPlus,
        [91] = Keys.OemOpenBrackets,
        [92] = Keys.OemPipe,
        [93] = Keys.OemCloseBrackets,
        [96] = Keys.OemTilde,
        [97] = Keys.A,
        [98] = Keys.B,
        [99] = Keys.C,
        [100] = Keys.D,
        [101] = Keys.E,
        [102] = Keys.F,
        [103] = Keys.G,
        [104] = Keys.H,
        [105] = Keys.I,
        [106] = Keys.J,
        [107] = Keys.K,
        [108] = Keys.L,
        [109] = Keys.M,
        [110] = Keys.N,
        [111] = Keys.O,
        [112] = Keys.P,
        [113] = Keys.Q,
        [114] = Keys.R,
        [115] = Keys.S,
        [116] = Keys.T,
        [117] = Keys.U,
        [118] = Keys.V,
        [119] = Keys.W,
        [120] = Keys.X,
        [121] = Keys.Y,
        [122] = Keys.Z,
        [127] = Keys.Delete,
        [1073741881] = Keys.CapsLock,
        [1073741882] = Keys.F1,
        [1073741883] = Keys.F2,
        [1073741884] = Keys.F3,
        [1073741885] = Keys.F4,
        [1073741886] = Keys.F5,
        [1073741887] = Keys.F6,
        [1073741888] = Keys.F7,
        [1073741889] = Keys.F8,
        [1073741890] = Keys.F9,
        [1073741891] = Keys.F10,
        [1073741892] = Keys.F11,
        [1073741893] = Keys.F12,
        [1073741894] = Keys.PrintScreen,
        [1073741895] = Keys.Scroll,
        [1073741896] = Keys.Pause,
        [1073741897] = Keys.Insert,
        [1073741898] = Keys.Home,
        [1073741899] = Keys.PageUp,
        [1073741901] = Keys.End,
        [1073741902] = Keys.PageDown,
        [1073741903] = Keys.Right,
        [1073741904] = Keys.Left,
        [1073741905] = Keys.Down,
        [1073741906] = Keys.Up,
        [1073741907] = Keys.NumLock,
        [1073741908] = Keys.Divide,
        [1073741909] = Keys.Multiply,
        [1073741910] = Keys.Subtract,
        [1073741911] = Keys.Add,
        [1073741912] = Keys.Enter,
        [1073741913] = Keys.NumPad1,
        [1073741914] = Keys.NumPad2,
        [1073741915] = Keys.NumPad3,
        [1073741916] = Keys.NumPad4,
        [1073741917] = Keys.NumPad5,
        [1073741918] = Keys.NumPad6,
        [1073741919] = Keys.NumPad7,
        [1073741920] = Keys.NumPad8,
        [1073741921] = Keys.NumPad9,
        [1073741922] = Keys.NumPad0,
        [1073741923] = Keys.Decimal,
        [1073741925] = Keys.Apps,
        [1073741928] = Keys.F13,
        [1073741929] = Keys.F14,
        [1073741930] = Keys.F15,
        [1073741931] = Keys.F16,
        [1073741932] = Keys.F17,
        [1073741933] = Keys.F18,
        [1073741934] = Keys.F19,
        [1073741935] = Keys.F20,
        [1073741936] = Keys.F21,
        [1073741937] = Keys.F22,
        [1073741938] = Keys.F23,
        [1073741939] = Keys.F24,
        [1073741951] = Keys.VolumeMute,
        [1073741952] = Keys.VolumeUp,
        [1073741953] = Keys.VolumeDown,
        [1073742040] = Keys.OemClear,
        [1073742044] = Keys.Decimal,
        [1073742048] = Keys.LeftControl,
        [1073742049] = Keys.LeftShift,
        [1073742050] = Keys.LeftAlt,
        [1073742051] = Keys.LeftWindows,
        [1073742052] = Keys.RightControl,
        [1073742053] = Keys.RightShift,
        [1073742054] = Keys.RightAlt,
        [1073742055] = Keys.RightWindows,
        [1073742082] = Keys.MediaNextTrack,
        [1073742083] = Keys.MediaPreviousTrack,
        [1073742084] = Keys.MediaStop,
        [1073742085] = Keys.MediaPlayPause,
        [1073742086] = Keys.VolumeMute,
        [1073742087] = Keys.SelectMedia,
        [1073742089] = Keys.LaunchMail,
        [1073742092] = Keys.BrowserSearch,
        [1073742093] = Keys.BrowserHome,
        [1073742094] = Keys.BrowserBack,
        [1073742095] = Keys.BrowserForward,
        [1073742096] = Keys.BrowserStop,
        [1073742097] = Keys.BrowserRefresh,
        [1073742098] = Keys.BrowserFavorites,
        [1073742106] = Keys.Sleep
    };

    public static Keys ToXna(int key) => Map.TryGetValue(key, out Keys value) ? value : Keys.None;
}