using MasterParking.Core;
using MasterParking.SaveSystem;
using UnityEngine;

namespace MasterParking.Audio
{
    /// <summary>
    /// App-level audio. Music survives screen changes; engine/gas/brake loops are
    /// started and stopped per level attempt. Volumes come from SettingsState.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        private AudioSource _music;
        private AudioSource _engine;
        private AudioSource _gas;
        private AudioSource _brake;
        private AudioSource _sfx;
        private SettingsState _settings;
        private bool _gameplayActive;

        public void Initialize(SettingsState settings)
        {
            _settings = settings ?? new SettingsState();

            // Audio must be audible on the menu too, not only when a gameplay
            // camera exists. The listener lives on the persistent app root.
            if (GetComponent<AudioListener>() == null)
            {
                gameObject.AddComponent<AudioListener>();
            }

            _music = CreateSource("Music", true);
            _engine = CreateSource("Engine", true);
            _gas = CreateSource("Gas", true);
            _brake = CreateSource("Brake", true);
            _sfx = CreateSource("Sfx", false);

            _music.clip = AudioClipFactory.MusicLoop();
            _engine.clip = AudioClipFactory.EngineLoop();
            _gas.clip = AudioClipFactory.GasLoop();
            _brake.clip = AudioClipFactory.BrakeLoop();

            _music.loop = true;
            _engine.loop = true;
            _gas.loop = true;
            _brake.loop = true;

            ApplySettings(_settings);
            _music.Play();
            _engine.volume = 0f;
            _gas.volume = 0f;
            _brake.volume = 0f;
            _engine.Play();
            _gas.Play();
            _brake.Play();
        }

        public void ApplySettings(SettingsState settings)
        {
            _settings = settings ?? _settings;
            if (_settings == null || _music == null)
            {
                return;
            }

            _music.volume = _settings.MusicVolume * 0.65f;
            _sfx.volume = 1f;
            if (!_gameplayActive)
            {
                _engine.volume = 0f;
            }
        }

        public void BeginGameplay()
        {
            _gameplayActive = true;
            if (_engine != null)
            {
                _engine.volume = 0f;
                if (!_engine.isPlaying)
                {
                    _engine.Play();
                }
            }
        }

        public void EndGameplay()
        {
            _gameplayActive = false;
            if (_engine != null)
            {
                _engine.volume = 0f;
            }

            if (_gas != null)
            {
                _gas.volume = 0f;
            }

            if (_brake != null)
            {
                _brake.volume = 0f;
            }
        }

        /// <summary>Called every frame by the running session.</summary>
        public void UpdateVehicle(float speedRatio, bool throttle, bool brake)
        {
            if (!_gameplayActive || _engine == null || _settings == null)
            {
                return;
            }

            float speed = Mathf.Clamp01(speedRatio);
            _engine.volume = _settings.EngineVolume * Mathf.Lerp(0.35f, 0.8f, speed);
            _engine.pitch = 0.75f + speed * 0.8f + (throttle ? 0.12f : 0f);
            _gas.volume = throttle ? _settings.EngineVolume * 0.35f : 0f;
            _brake.volume = brake && speed > 0.03f ? _settings.EngineVolume * 0.3f : 0f;
        }

        public void SetPaused(bool paused)
        {
            if (_engine == null)
            {
                return;
            }

            if (paused)
            {
                _engine.volume = 0f;
                _gas.volume = 0f;
                _brake.volume = 0f;
            }

            _music.volume = paused ? _settings.MusicVolume * 0.25f : _settings.MusicVolume * 0.65f;
        }

        public void PlayClick() => PlayOneShot(AudioClipFactory.Click(), 0.8f);
        public void PlayCrash() => PlayOneShot(AudioClipFactory.Crash(), 1f);
        public void PlayWin() => PlayOneShot(AudioClipFactory.Win(), 1f);
        public void PlayLose() => PlayOneShot(AudioClipFactory.Lose(), 1f);

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (_sfx == null || clip == null || _settings == null)
            {
                return;
            }

            _sfx.PlayOneShot(clip, volume * _settings.SfxVolume);
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
