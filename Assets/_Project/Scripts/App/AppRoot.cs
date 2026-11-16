using MasterParking.Audio;
using MasterParking.Core;
using MasterParking.LevelSystem;
using MasterParking.SaveSystem;
using UnityEngine;

namespace MasterParking.App
{
    /// <summary>
    /// Persistent application composition root: save data, levels, audio and the
    /// state machine are created once and shared by every screen.
    /// This is the one intentional singleton; gameplay systems receive it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class AppRoot : MonoBehaviour
    {
        public static AppRoot Instance { get; private set; }

        public SaveManager Save { get; private set; }
        public LevelRepository Levels { get; private set; }
        public AudioService Audio { get; private set; }
        public GameStateMachine StateMachine { get; private set; }
        public AppFlow Flow { get; private set; }
        public SettingsState Settings => Save.Settings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Levels = LevelRepository.LoadDefault();
            Save = new SaveManager(new FileSaveStorage(), Mathf.Max(1, Levels.Count));
            StateMachine = new GameStateMachine();

            Audio = gameObject.AddComponent<AudioService>();
            Audio.Initialize(Save.Settings);

            EnsureFallbackCamera();

            Flow = gameObject.AddComponent<AppFlow>();
            Flow.Initialize(this);

            ApplyMobileFrameRate();
        }

        private void Start()
        {
            Flow.ShowMainMenu();
        }

        public void SaveSettings(SettingsState settings)
        {
            Save.ApplySettings(settings);
            Audio.ApplySettings(Save.Settings);
        }

        private void OnApplicationPause(bool paused)
        {
            Save.Save();

            if (paused)
            {
                Flow?.PauseForLifecycle();
            }
        }

        private void OnApplicationQuit()
        {
            Save?.Save();
        }

        private void EnsureFallbackCamera()
        {
            // Menus are UI-only, but the Game view still needs a camera to clear
            // the frame. Gameplay creates its own camera at a higher depth.
            if (GetComponent<Camera>() != null)
            {
                return;
            }

            var fallback = gameObject.AddComponent<Camera>();
            fallback.clearFlags = CameraClearFlags.SolidColor;
            fallback.backgroundColor = new Color(0.04f, 0.06f, 0.1f);
            fallback.cullingMask = 0;
            fallback.orthographic = true;
            fallback.orthographicSize = 5f;
            fallback.depth = -100f;
        }

        private static void ApplyMobileFrameRate()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // The game is landscape-only, but the player settings may be edited
            // or reset. Enforcing the orientation at runtime keeps the UI composable
            // on both Android and iOS without platform-specific gameplay code.
            if (Application.isMobilePlatform)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
        }
    }
}
