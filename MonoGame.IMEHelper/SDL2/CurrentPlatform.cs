// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System;

namespace MonoGame.IMEHelper;

internal static class CurrentPlatform
{
    private static OS? s_os;

    [DllImport("libc")]
    private static extern int uname(IntPtr buf);

    private static OS Init()
    {
        PlatformID pid = Environment.OSVersion.Platform;

        switch (pid)
        {
            case PlatformID.Win32NT:
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.WinCE:
                return OS.Windows;
            case PlatformID.MacOSX:
                return OS.MacOSX;
            case PlatformID.Unix:

                // Mac can return a value of Unix sometimes, We need to double check it.
                IntPtr buf = IntPtr.Zero;
                try
                {
                    buf = Marshal.AllocHGlobal(8192);

                    if (uname(buf) == 0)
                    {
                        string? sos = Marshal.PtrToStringAnsi(buf);
                        if (sos == "Darwin")
                            return OS.MacOSX;
                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    if (buf != IntPtr.Zero)
                        Marshal.FreeHGlobal(buf);
                }

                return OS.Linux;
            case PlatformID.Xbox:
            case PlatformID.Other:
            default:
                return OS.Unknown;
        }
    }

    public static OS OS => s_os ??= Init();

    public static string Rid => OS switch
    {
        OS.Windows when Environment.Is64BitProcess => "win-x64",
        OS.Windows when !Environment.Is64BitProcess => "win-x86",
        OS.Linux => "linux-x64",
        OS.MacOSX => "osx",
        _ => "unknown"
    };
}