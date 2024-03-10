using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    [LibraryImport("SDL2", EntryPoint = "SDL_StartTextInput")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void StartTextInput();

    [LibraryImport("SDL2", EntryPoint = "SDL_StopTextInput")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void StopTextInput();

    [LibraryImport("SDL2", EntryPoint = "SDL_SetTextInputRect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SetTextInputRect(ref Rectangle rect);
}