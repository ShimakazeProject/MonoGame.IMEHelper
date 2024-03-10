using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.IMEHelper;

internal class FuncLoader
{
    private static class Windows
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(string lpszLib);
    }

    private static class Linux
    {
#pragma warning disable CA2101
        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        [DllImport("libdl.so.2")]
        public static extern IntPtr dlopen(string path, int flags);

        [DllImport("libdl.so.2")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
        // ReSharper restore StringLiteralTypo
        // ReSharper restore IdentifierTypo
#pragma warning restore CA2101
    }

    // ReSharper disable once InconsistentNaming
    private static class OSX
    {
#pragma warning disable CA2101
        // ReSharper disable IdentifierTypo
        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern IntPtr dlopen(string path, int flags);

        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
        // ReSharper restore IdentifierTypo
#pragma warning restore CA2101
    }

    // ReSharper disable once IdentifierTypo
    // ReSharper disable once InconsistentNaming
    private const int RTLD_LAZY = 0x0001;

    public static IntPtr LoadLibraryExt(string libName)
    {
        IntPtr ret;
        var assemblyLocation = Path.GetDirectoryName(typeof(FuncLoader).Assembly.Location) ?? "./";

        // Try .NET Framework / mono locations
        if (CurrentPlatform.OS == OS.MacOSX)
        {
            ret = LoadLibrary(Path.Combine(assemblyLocation, libName));

            // Look in Frameworks for .app bundles
            if (ret == IntPtr.Zero)
                ret = LoadLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libName));
        }
        else
        {
            ret = LoadLibrary(Environment.Is64BitProcess
                ? Path.Combine(assemblyLocation, "x64", libName)
                : Path.Combine(assemblyLocation, "x86", libName));
        }

        // Try .NET Core development locations
        if (ret == IntPtr.Zero)
            ret = LoadLibrary(Path.Combine(assemblyLocation, "runtimes", CurrentPlatform.Rid, "native", libName));

        // Try current folder (.NET Core will copy it there after publish) or system library
        if (ret == IntPtr.Zero)
            ret = LoadLibrary(libName);

        // Well, all failed, PANIC!!!
        if (ret == IntPtr.Zero)
            throw new Exception("Failed to load library: " + libName);

        return ret;
    }

    private static IntPtr LoadLibrary(string libName)
    {
        return CurrentPlatform.OS switch
        {
            OS.Windows => Windows.LoadLibraryW(libName),
            OS.MacOSX => OSX.dlopen(libName, RTLD_LAZY),
            _ => Linux.dlopen(libName, RTLD_LAZY)
        };
    }

    public static T? LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
        where T : Delegate
    {
        IntPtr ret = CurrentPlatform.OS switch
        {
            OS.Windows => Windows.GetProcAddress(library, function),
            OS.MacOSX => OSX.dlsym(library, function),
            _ => Linux.dlsym(library, function)
        };

        if (ret != IntPtr.Zero)
            return Marshal.GetDelegateForFunctionPointer<T>(ret);

        if (throwIfNotFound)
            throw new EntryPointNotFoundException(function);

        return default;
    }
}