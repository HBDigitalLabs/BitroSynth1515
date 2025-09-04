using System.Runtime.InteropServices;

namespace RustPlaybackNative
{
    public static class RustPlaybackEngine
    {
        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int audio_engine_init();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void audio_engine_deinit();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int play();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stop();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte get_playback_status();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int set_file_path(string new_file_path);

    }
}