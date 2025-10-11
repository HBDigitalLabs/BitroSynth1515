using System.Runtime.InteropServices;

namespace RustPlaybackNative
{
    public static class RustPlaybackEngine
    {
        public enum EngineStatus : sbyte
        {
            Success = 0,
            Error = -1,
            InvalidStartPosition = -2
        }

        // 0: Success
        // -1: General error (I/O, lock, stream failure)
        // -2: Invalid start position (beyond file length)
    
        public static int startPositionMs = 0;
        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int audio_engine_init();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void audio_engine_deinit();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int play(int start_position_ms);

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stop();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte get_playback_status();

        [DllImport("rust_playback_engine", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int set_file_path(string new_file_path);

    }
}