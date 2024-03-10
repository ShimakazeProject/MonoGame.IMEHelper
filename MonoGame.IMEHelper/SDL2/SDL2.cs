#if false
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#else
using System;
using System.IO;
using System.Runtime.InteropServices;

using Windows.Win32;
#endif

namespace MonoGame.IMEHelper;


internal static partial class Sdl
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
#if false
    [LibraryImport("SDL2", EntryPoint = "SDL_StartTextInput")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void StartTextInput();

    [LibraryImport("SDL2", EntryPoint = "SDL_StopTextInput")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void StopTextInput();

    [LibraryImport("SDL2", EntryPoint = "SDL_SetTextInputRect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SetTextInputRect(in Rectangle rect);
#else

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void d_sdl_starttextinput();
    public static d_sdl_starttextinput StartTextInput => FuncLoader.LoadFunction<d_sdl_starttextinput>(NativeLibrary, "SDL_StartTextInput");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void d_sdl_stoptextinput();
    public static d_sdl_stoptextinput StopTextInput => FuncLoader.LoadFunction<d_sdl_stoptextinput>(NativeLibrary, "SDL_StopTextInput");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void d_sdl_settextinputrect(in Rectangle rect);
    public static d_sdl_settextinputrect SetTextInputRect => FuncLoader.LoadFunction<d_sdl_settextinputrect>(NativeLibrary, "SDL_SetTextInputRect");

    private static readonly SafeHandle NativeLibrary = GetNativeLibrary();

    private static SafeHandle GetNativeLibrary()
    {
        return CurrentPlatform.OS switch
        {
            OS.Windows => FuncLoader.LoadLibraryExt("SDL2.dll"),
            OS.Linux => FuncLoader.LoadLibraryExt("libSDL2-2.0.so.0"),
            OS.MacOSX => FuncLoader.LoadLibraryExt("libSDL2-2.0.0.dylib"),
            _ => FuncLoader.LoadLibraryExt("sdl2")
        };
    }

    private class FuncLoader
    {
        private sealed class LinuxHandle(nint invalidHandleValue, bool ownsHandle) : SafeHandle(invalidHandleValue, ownsHandle)
        {
            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                dlclose(handle);
                return true;
            }

            public static LinuxHandle LoadLibrary(string libname) => new(dlopen(libname, RTLD_LAZY), true);
            public static nint GetProcAddress(SafeHandle library, string function) => dlsym(library.DangerousGetHandle(), function);


            [DllImport("libdl.so.2")]
            public static extern nint dlopen(string path, int flags);

            [DllImport("libdl.so.2")]
            public static extern nint dlsym(nint handle, string symbol);

            [DllImport("libdl.so.2")]
            public static extern nint dlclose(nint handle);
        }

        private sealed class OSXHandle(nint invalidHandleValue, bool ownsHandle) : SafeHandle(invalidHandleValue, ownsHandle)
        {
            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                dlclose(handle);
                return true;
            }

            public static OSXHandle LoadLibrary(string libname) => new(dlopen(libname, RTLD_LAZY), true);
            public static nint GetProcAddress(SafeHandle library, string function) => dlsym(library.DangerousGetHandle(), function);


            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern nint dlopen(string path, int flags);

            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern nint dlsym(nint handle, string symbol);

            [DllImport("/usr/lib/libSystem.dylib")]
            public static extern nint dlclose(nint handle);
        }


        private const int RTLD_LAZY = 0x0001;

        public static SafeHandle LoadLibraryExt(string libname)
        {
            var assemblyLocation = Path.GetDirectoryName(typeof(FuncLoader).Assembly.Location) ?? "./";

            SafeHandle ret;
            // Try .NET Framework / mono locations
            if (CurrentPlatform.OS == OS.MacOSX)
            {
                ret = LoadLibrary(Path.Combine(assemblyLocation, libname));

                // Look in Frameworks for .app bundles
                if (ret.IsInvalid)
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libname));
            }
            else
            {
                if (Environment.Is64BitProcess)
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "x64", libname));
                else
                    ret = LoadLibrary(Path.Combine(assemblyLocation, "x86", libname));
            }

            // Try .NET Core development locations
            if (ret.IsInvalid)
                ret = LoadLibrary(Path.Combine(assemblyLocation, "runtimes", CurrentPlatform.Rid, "native", libname));

            // Try current folder (.NET Core will copy it there after publish) or system library
            if (ret.IsInvalid)
                ret = LoadLibrary(libname);

            // Welp, all failed, PANIC!!!
            if (ret.IsInvalid)
                throw new Exception("Failed to load library: " + libname);

            return ret;
        }

        public static SafeHandle LoadLibrary(string libname)
        {
            return CurrentPlatform.OS switch
            {
                OS.Windows => PInvoke.LoadLibrary(libname),
                OS.MacOSX => OSXHandle.LoadLibrary(libname),
                OS.Linux => LinuxHandle.LoadLibrary(libname)
            };
        }

        public static T LoadFunction<T>(SafeHandle library, string function, bool throwIfNotFound = false)
        {
            var ret = CurrentPlatform.OS switch
            {
                OS.Windows => (nint)PInvoke.GetProcAddress(library, function),
                OS.MacOSX => OSXHandle.GetProcAddress(library, function),
                _ => LinuxHandle.GetProcAddress(library, function),
            };
            return ret switch
            {
                0 => throwIfNotFound
                    ? throw new EntryPointNotFoundException(function)
                    : default!,
                _ => Marshal.GetDelegateForFunctionPointer<T>(ret)
            };
        }
    }

    private enum OS
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }

    private static class CurrentPlatform
    {
        private static bool init = false;
        private static OS os;

        [DllImport("libc")]
        static extern int uname(nint buf);

        private static void Init()
        {
            if (!init)
            {
                PlatformID pid = Environment.OSVersion.Platform;

                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        os = OS.Windows;
                        break;
                    case PlatformID.MacOSX:
                        os = OS.MacOSX;
                        break;
                    case PlatformID.Unix:

                        // Mac can return a value of Unix sometimes, We need to double check it.
                        nint buf = 0;
                        try
                        {
                            buf = Marshal.AllocHGlobal(8192);

                            if (uname(buf) == 0)
                            {
                                string sos = Marshal.PtrToStringAnsi(buf);
                                if (sos == "Darwin")
                                {
                                    os = OS.MacOSX;
                                    return;
                                }
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (buf != 0)
                                Marshal.FreeHGlobal(buf);
                        }

                        os = OS.Linux;
                        break;
                    default:
                        os = OS.Unknown;
                        break;
                }

                init = true;
            }
        }

        public static OS OS
        {
            get
            {
                Init();
                return os;
            }
        }

        public static string Rid
        {
            get
            {
                return OS switch
                {
                    OS.Windows when Environment.Is64BitProcess => "win-x64",
                    OS.Windows when !Environment.Is64BitProcess => "win-x86",
                    OS.Linux => "linux-x64",
                    OS.MacOSX => "osx",
                    _ => "unknown"
                };
            }
        }
    }
#endif
}