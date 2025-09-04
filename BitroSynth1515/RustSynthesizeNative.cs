using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RustSynthesizeNative
{
    public static class RustSynthesizeEngine
    {
        [DllImport("rust_synthesize_engine", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int synthesize(
            string input_1,
            string input_2,
            string input_3,
            string input_4,
            byte bit_8,
            string output_path
        );


        [DllImport("rust_synthesize_engine", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int set_sample_rate(
            int new_sample_rate
        );

        public static string cachePath = "Cache.wav";
        public static byte bit8Status = 1;
        

        public static Dictionary<string, float> noteFrequency = new Dictionary<string, float>(){{ "C0", 16.35f }, { "C#0", 17.32f }, { "D0", 18.35f }, { "D#0", 19.45f }, { "E0", 20.6f }, { "F0", 21.83f }, { "F#0", 23.12f }, { "G0", 24.5f },
        { "G#0", 25.96f }, { "A0", 27.5f }, { "A#0", 29.14f }, { "B0", 30.87f }, { "C1", 32.7f }, { "C#1", 34.65f }, { "D1", 36.71f }, { "D#1", 38.89f },
        { "E1", 41.2f }, { "F1", 43.65f }, { "F#1", 46.25f }, { "G1", 49 }, { "G#1", 51.91f }, { "A1", 55 }, { "A#1", 58.27f }, { "B1", 61.74f },
        { "C2", 65.41f }, { "C#2", 69.3f }, { "D2", 73.42f }, { "D#2", 77.78f }, { "E2", 82.41f }, { "F2", 87.31f }, { "F#2", 92.5f }, { "G2", 98 },
        { "G#2", 103.83f}, { "A2", 110 }, { "A#2", 116.54f }, { "B2", 123.47f }, { "C3", 130.81f },{ "C#3", 138.59f }, { "D3", 146.83f },{ "D#3", 155.56f }, { "E3", 164.81f }, { "F3", 174.61f }, { "F#3", 185 },
        { "G3", 196 },{ "G#3", 207.65f }, { "A3", 220 },{ "A#3", 233.08f }, { "B3", 246.94f }, { "C4", 261.63f }, { "C#4", 277.18f }, { "D4", 293.66f }, { "D#4", 311.13f }, { "E4", 329.63f },
        { "F4", 349.23f }, { "F#4", 369.99f }, { "G4", 392 }, { "G#4", 415.3f }, { "A4", 440 }, { "A#4", 466.16f }, { "B4", 493.88f },
        { "C5", 523.25f }, { "C#5", 554.37f}, { "D5", 587.33f }, { "D#5", 622.25f }, { "E5", 659.26f}, { "F5", 698.46f }, { "F#5", 739.99f }, { "G5", 783.99f },
        { "G#5", 830.61f }, { "A5", 880 }, { "A#5", 932.33f}, { "B5", 987.77f }, { "C6", 1046.5f }, { "C#6", 1108.73f }, { "D6", 1174.66f }, { "D#6", 1244.51f },
        { "E6", 1318.51f }, { "F6", 1396.91f}, { "F#6", 1479.98f }, { "G6", 1567.98f }, { "G#6", 1661.22f }, { "A6", 1760 }, { "A#6", 1864.66f}, { "B6", 1975.53f },
        { "C7", 2093 }, { "C#7",2217.46f }, { "D7", 2349.32f }, { "D#7", 2489.02f }, { "E7", 2637.02f }, { "F7", 2793.83f }, { "F#7", 2959.96f }, { "G7", 3135.96f },
        { "G#7", 3322.44f }, { "A7", 3520 }, { "A#7", 3729.31f }, { "B7", 3951.07f }, { "C8", 4186.01f }, { "C#8", 4434.92f }, { "D8", 4698.64f }, { "D#8", 4978.03f },
        { "E8", 5274.04f }, { "F8", 5587.65f }, { "F#8", 5919.91f }, { "G8", 6271.93f }, { "G#8", 6644.88f }, { "A8", 7040 }, { "A#8", 7458.62f }, { "B8", 7902.13f }};

    }
}