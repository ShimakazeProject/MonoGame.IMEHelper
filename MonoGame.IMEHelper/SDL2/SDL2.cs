using System;
using System.Runtime.InteropServices;

namespace MonoGame.IMEHelper;

internal static class Sdl
{
    private static readonly IntPtr NativeLibrary = GetNativeLibrary();

    private static IntPtr GetNativeLibrary()
    {
        return CurrentPlatform.OS switch
        {
            OS.Windows => FuncLoader.LoadLibraryExt("SDL2.dll"),
            OS.Linux => FuncLoader.LoadLibraryExt("libSDL2-2.0.so.0"),
            OS.MacOSX => FuncLoader.LoadLibraryExt("libSDL2-2.0.0.dylib"),
            _ => FuncLoader.LoadLibraryExt("sdl2")
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DSdlStartTextInput();
    public static readonly DSdlStartTextInput? StartTextInput = FuncLoader.LoadFunction<DSdlStartTextInput>(NativeLibrary, "SDL_StartTextInput");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DSdlStopTextInput();
    public static readonly DSdlStopTextInput? StopTextInput = FuncLoader.LoadFunction<DSdlStopTextInput>(NativeLibrary, "SDL_StopTextInput");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DSdlSetTextInputRect(ref Rectangle rect);
    public static readonly DSdlSetTextInputRect? SetTextInputRect = FuncLoader.LoadFunction<DSdlSetTextInputRect>(NativeLibrary, "SDL_SetTextInputRect");
}