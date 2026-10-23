using UnityEngine;

namespace TwoMoons
{
    /// <summary>Erzeugt 8-Bit-Platzhalter-Sounds und eine kleine Chiptune-Melodie per Code.</summary>
    public static class PlaceholderSounds
    {
        public const int SampleRate = 22050;

        enum Wave { Square, Pulse, Triangle, Noise }

        /// <summary>Kleiner Synthesizer, der Töne in einen Puffer mischt.</summary>
        class Synth
        {
            readonly float[] data;

            public Synth(float seconds) => data = new float[Mathf.CeilToInt(seconds * SampleRate)];

            public void Note(float start, float duration, float frequency, Wave wave, float volume,
                float endFrequency = -1f, bool decay = true)
            {
                int first = Mathf.RoundToInt(start * SampleRate);
                int count = Mathf.RoundToInt(duration * SampleRate);
                int attack = Mathf.Max(1, SampleRate / 250);
                double phase = 0;
                float noiseValue = 0f;
                var random = new System.Random(first + 1);

                for (int i = 0; i < count; i++)
                {
                    int index = first + i;
                    if (index >= data.Length) break;

                    float t = i / (float)count;
                    float f = endFrequency > 0f ? Mathf.Lerp(frequency, endFrequency, t) : frequency;
                    phase += f / SampleRate;
                    if (phase >= 1.0)
                    {
                        phase -= System.Math.Floor(phase);
                        noiseValue = (float)(random.NextDouble() * 2.0 - 1.0);
                    }

                    float sample;
                    switch (wave)
                    {
                        case Wave.Square: sample = phase < 0.5 ? 1f : -1f; break;
                        case Wave.Pulse: sample = phase < 0.25 ? 1f : -1f; break;
                        case Wave.Triangle: sample = 4f * Mathf.Abs((float)phase - 0.5f) - 1f; break;
                        default: sample = noiseValue; break;
                    }

                    float envelope = Mathf.Min(1f, i / (float)attack);
                    envelope *= decay ? Mathf.Pow(1f - t, 1.5f) : (t > 0.85f ? (1f - t) / 0.15f : 1f);
                    data[index] += sample * volume * envelope;
                }
            }

            public float[] Finish()
            {
                for (int i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
                return data;
            }
        }

        static float Midi(int note) => 440f * Mathf.Pow(2f, (note - 69) / 12f);

        public static AudioClip CreateClip(string key)
        {
            var samples = CreateSamples(key);
            var clip = AudioClip.Create(key, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static float[] CreateSamples(string key)
        {
            Synth s;
            switch (key)
            {
                case Sfx.Register:
                    s = new Synth(0.45f);
                    s.Note(0f, 0.07f, Midi(83), Wave.Square, 0.25f, decay: false);
                    s.Note(0.07f, 0.35f, Midi(88), Wave.Square, 0.25f);
                    s.Note(0f, 0.05f, 6000f, Wave.Noise, 0.12f);
                    break;
                case Sfx.Doorbell:
                    s = new Synth(0.8f);
                    s.Note(0f, 0.4f, Midi(76), Wave.Triangle, 0.45f);
                    s.Note(0.35f, 0.45f, Midi(72), Wave.Triangle, 0.45f);
                    break;
                case Sfx.Pickup:
                    s = new Synth(0.12f);
                    s.Note(0f, 0.12f, 300f, Wave.Pulse, 0.2f, 700f);
                    break;
                case Sfx.Drop:
                    s = new Synth(0.16f);
                    s.Note(0f, 0.14f, 420f, Wave.Pulse, 0.2f, 140f);
                    s.Note(0f, 0.08f, 1500f, Wave.Noise, 0.15f);
                    break;
                case Sfx.Stock:
                    s = new Synth(0.3f);
                    s.Note(0f, 0.08f, Midi(72), Wave.Pulse, 0.2f);
                    s.Note(0.07f, 0.08f, Midi(76), Wave.Pulse, 0.2f);
                    s.Note(0.14f, 0.14f, Midi(79), Wave.Pulse, 0.2f);
                    break;
                case Sfx.Clean:
                    s = new Synth(0.3f);
                    s.Note(0f, 0.3f, 2500f, Wave.Noise, 0.18f, 7000f);
                    break;
                case Sfx.Order:
                    s = new Synth(0.22f);
                    s.Note(0f, 0.08f, Midi(81), Wave.Square, 0.18f, decay: false);
                    s.Note(0.1f, 0.1f, Midi(88), Wave.Square, 0.18f);
                    break;
                case Sfx.Build:
                    s = new Synth(0.25f);
                    s.Note(0f, 0.12f, 140f, Wave.Square, 0.3f, 90f);
                    s.Note(0f, 0.1f, 900f, Wave.Noise, 0.2f);
                    s.Note(0.1f, 0.12f, 130f, Wave.Square, 0.2f, 80f);
                    break;
                case Sfx.Error:
                    s = new Synth(0.3f);
                    s.Note(0f, 0.12f, 160f, Wave.Square, 0.2f, decay: false);
                    s.Note(0.15f, 0.14f, 130f, Wave.Square, 0.2f);
                    break;
                case Sfx.Quest:
                    s = new Synth(0.6f);
                    s.Note(0f, 0.1f, Midi(72), Wave.Square, 0.18f, decay: false);
                    s.Note(0.1f, 0.1f, Midi(76), Wave.Square, 0.18f, decay: false);
                    s.Note(0.2f, 0.1f, Midi(79), Wave.Square, 0.18f, decay: false);
                    s.Note(0.3f, 0.3f, Midi(84), Wave.Square, 0.18f);
                    break;
                case Sfx.Fanfare:
                    s = new Synth(1.4f);
                    int[] fanfare = { 72, 76, 79, 84, 79, 84 };
                    float[] lengths = { 0.12f, 0.12f, 0.12f, 0.24f, 0.12f, 0.6f };
                    float time = 0f;
                    for (int i = 0; i < fanfare.Length; i++)
                    {
                        s.Note(time, lengths[i], Midi(fanfare[i]), Wave.Square, 0.18f, decay: i == fanfare.Length - 1);
                        s.Note(time, lengths[i], Midi(fanfare[i] - 24), Wave.Triangle, 0.3f, decay: i == fanfare.Length - 1);
                        time += lengths[i];
                    }
                    break;
                case Sfx.DayEnd:
                    s = new Synth(0.9f);
                    s.Note(0f, 0.2f, Midi(79), Wave.Triangle, 0.4f);
                    s.Note(0.2f, 0.2f, Midi(76), Wave.Triangle, 0.4f);
                    s.Note(0.4f, 0.5f, Midi(72), Wave.Triangle, 0.4f);
                    break;
                case Sfx.UiClick:
                    s = new Synth(0.04f);
                    s.Note(0f, 0.04f, 1100f, Wave.Pulse, 0.15f);
                    break;
                case Sfx.Dialog:
                    s = new Synth(0.05f);
                    s.Note(0f, 0.05f, Midi(79), Wave.Pulse, 0.12f);
                    break;
                case Sfx.Music:
                    return CreateMusic();
                default:
                    s = new Synth(0.1f);
                    s.Note(0f, 0.1f, 440f, Wave.Square, 0.2f);
                    break;
            }
            return s.Finish();
        }

        /// <summary>16 Takte gemütlicher Chiptune in C-Dur, nahtlos wiederholbar (ca. 32 Sekunden).</summary>
        static float[] CreateMusic()
        {
            const float eighth = 0.25f; // 120 BPM
            const float bar = eighth * 8f;

            // Grundton (MIDI) und Dur/Moll pro Takt: C Am F G | C Am Dm G | F G Em Am | F G C C
            int[] roots = { 48, 45, 53, 55, 48, 45, 50, 55, 53, 55, 52, 45, 53, 55, 48, 48 };
            bool[] minor = { false, true, false, false, false, true, true, false, false, false, true, true, false, false, false, false };
            // Melodie: Stufe im Akkord (0 = Grundton, 1 = Terz, 2 = Quinte, 3 = Oktave) pro Viertel, -1 = Pause.
            int[][] melody =
            {
                new[] { 2, 3, 2, 1 }, new[] { 1, 2, -1, 1 }, new[] { 0, 1, 2, 3 }, new[] { 3, 2, 1, -1 },
                new[] { 2, 3, 2, 1 }, new[] { 1, 2, 3, 2 }, new[] { 1, 0, 1, 2 }, new[] { 2, -1, 3, -1 },
                new[] { 3, 2, 3, 1 }, new[] { 2, 1, 2, 3 }, new[] { 1, 2, 1, 0 }, new[] { 0, 1, 2, -1 },
                new[] { 3, 2, 1, 2 }, new[] { 3, -1, 2, 1 }, new[] { 2, 1, 0, 1 }, new[] { 0, -1, -1, -1 },
            };
            int[] arpeggio = { 0, 1, 2, 3, 2, 1, 0, 1 };

            var s = new Synth(bar * roots.Length);
            for (int b = 0; b < roots.Length; b++)
            {
                float start = b * bar;
                int root = roots[b];
                int[] chord = { root, root + (minor[b] ? 3 : 4), root + 7, root + 12 };

                // Bass auf den Vierteln
                int[] bass = { root - 12, root - 12, root - 5, root - 12 };
                for (int q = 0; q < 4; q++)
                    s.Note(start + q * 2 * eighth, eighth * 1.6f, Midi(bass[q]), Wave.Triangle, 0.28f, decay: false);

                // Arpeggio in Achteln
                for (int e = 0; e < 8; e++)
                    s.Note(start + e * eighth, eighth * 0.9f, Midi(chord[arpeggio[e]] + 12), Wave.Pulse, 0.05f);

                // Melodie in Vierteln
                for (int q = 0; q < 4; q++)
                {
                    int step = melody[b][q];
                    if (step < 0) continue;
                    s.Note(start + q * 2 * eighth, eighth * 1.8f, Midi(chord[step] + 24), Wave.Square, 0.07f);
                }

                // Leises Hi-Hat auf den Offbeats
                for (int e = 1; e < 8; e += 2)
                    s.Note(start + e * eighth, 0.04f, 8000f, Wave.Noise, 0.04f);
            }
            return s.Finish();
        }
    }
}
