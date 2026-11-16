using System;
using UnityEngine;

namespace MasterParking.Audio
{
    /// <summary>
    /// Procedural placeholder audio. Keeping clips synthesized avoids shipping
    /// third-party recordings while still exercising every audio hook.
    /// </summary>
    public static class AudioClipFactory
    {
        public const int SampleRate = 44100;

        private static AudioClip _click;
        private static AudioClip _crash;
        private static AudioClip _win;
        private static AudioClip _lose;
        private static AudioClip _engine;
        private static AudioClip _gas;
        private static AudioClip _brake;
        private static AudioClip _music;

        public static AudioClip Click() => _click ??= BuildClick();
        public static AudioClip Crash() => _crash ??= BuildCrash();
        public static AudioClip Win() => _win ??= BuildWin();
        public static AudioClip Lose() => _lose ??= BuildLose();
        public static AudioClip EngineLoop() => _engine ??= BuildEngine();
        public static AudioClip GasLoop() => _gas ??= BuildGas();
        public static AudioClip BrakeLoop() => _brake ??= BuildBrake();
        public static AudioClip MusicLoop() => _music ??= BuildMusic();

        private static AudioClip BuildClick()
        {
            const float duration = 0.08f;
            float[] samples = new float[(int)(SampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 55f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(1300f, 700f, t / duration) * t) * envelope * 0.35f;
            }

            return Create("UI_Click", samples);
        }

        private static AudioClip BuildCrash()
        {
            const float duration = 0.5f;
            float[] samples = new float[(int)(SampleRate * duration)];
            var random = new System.Random(1234);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 9f);
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                float thump = Mathf.Sin(2f * Mathf.PI * 72f * t) * Mathf.Exp(-t * 6f);
                samples[i] = Mathf.Clamp((noise * 0.45f + thump * 0.55f) * envelope, -1f, 1f) * 0.8f;
            }

            return Create("Crash", samples);
        }

        private static AudioClip BuildWin()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
            const float noteDuration = 0.18f;
            float[] samples = new float[(int)(SampleRate * noteDuration * notes.Length)];
            for (int n = 0; n < notes.Length; n++)
            {
                int start = (int)(noteDuration * SampleRate * n);
                int length = (int)(noteDuration * SampleRate);
                for (int i = 0; i < length && start + i < samples.Length; i++)
                {
                    float t = (float)i / SampleRate;
                    float envelope = Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 6f);
                    samples[start + i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * envelope * 0.35f;
                }
            }

            return Create("Win", samples);
        }

        private static AudioClip BuildLose()
        {
            float[] notes = { 392f, 329.63f, 261.63f };
            const float noteDuration = 0.22f;
            float[] samples = new float[(int)(SampleRate * noteDuration * notes.Length)];
            for (int n = 0; n < notes.Length; n++)
            {
                int start = (int)(noteDuration * SampleRate * n);
                int length = (int)(noteDuration * SampleRate);
                for (int i = 0; i < length && start + i < samples.Length; i++)
                {
                    float t = (float)i / SampleRate;
                    float envelope = Mathf.Min(1f, t * 30f) * Mathf.Exp(-t * 3.5f);
                    samples[start + i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * envelope * 0.3f;
                }
            }

            return Create("Lose", samples);
        }

        private static AudioClip BuildEngine()
        {
            const float duration = 1f;
            float[] samples = new float[(int)(SampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float fundamental = Mathf.Sin(2f * Mathf.PI * 58f * t);
                float second = Mathf.Sin(2f * Mathf.PI * 116f * t) * 0.45f;
                float third = Mathf.Sin(2f * Mathf.PI * 174f * t) * 0.2f;
                float pulse = Mathf.Sin(2f * Mathf.PI * 29f * t) * 0.12f;
                samples[i] = (fundamental * 0.5f + second + third + pulse) * 0.3f;
            }

            return Create("Engine", samples, true);
        }

        private static AudioClip BuildGas()
        {
            const float duration = 0.6f;
            float[] samples = new float[(int)(SampleRate * duration)];
            var random = new System.Random(77);
            float filtered = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, noise, 0.08f);
                samples[i] = filtered * 0.33f;
            }

            return Create("Gas", samples, true);
        }

        private static AudioClip BuildBrake()
        {
            const float duration = 0.5f;
            float[] samples = new float[(int)(SampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float wobble = Mathf.Sin(2f * Mathf.PI * 7f * t) * 40f;
                samples[i] = Mathf.Sin(2f * Mathf.PI * (1500f + wobble) * t) * 0.08f;
            }

            return Create("Brake", samples, true);
        }

        private static AudioClip BuildMusic()
        {
            const float duration = 8f;
            float[] samples = new float[(int)(SampleRate * duration)];
            float[] chordRoots = { 130.81f, 174.61f, 196f, 146.83f };
            float[] arpeggio = { 261.63f, 329.63f, 392f, 523.25f, 392f, 329.63f };
            const float beat = 0.5f;

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                int chord = Mathf.FloorToInt(t / 2f) % chordRoots.Length;
                float root = chordRoots[chord];
                float bass = Mathf.Sin(2f * Mathf.PI * root * t) * 0.1f;

                int beatIndex = Mathf.FloorToInt(t / beat) % arpeggio.Length;
                float noteTime = t % beat;
                float note = Mathf.Sin(2f * Mathf.PI * arpeggio[beatIndex] * t) *
                             Mathf.Exp(-noteTime * 4f) * 0.055f;

                samples[i] = bass + note;
            }

            return Create("Music", samples, true);
        }

        private static AudioClip Create(string name, float[] samples, bool loop = false)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
