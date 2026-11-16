using MasterParking.Core;
using MasterParking.UI;
using UnityEngine;

namespace MasterParking.App
{
    /// <summary>
    /// Screen coordinator. Gameplay, level select and the main menu are UI states
    /// over one bootstrap scene, which avoids fragile scene references while the
    /// central GameStateMachine still owns legal flow.
    /// </summary>
    public sealed class AppFlow : MonoBehaviour
    {
        private AppRoot _app;
        private UiRoot _ui;
        private GameplayScreen _gameplay;
        private LevelDefinition _currentLevel;

        public LevelDefinition CurrentLevel => _currentLevel;

        public void Initialize(AppRoot app)
        {
            _app = app;
        }

        public void ShowMainMenu()
        {
            Cleanup();
            _currentLevel = null;
            _app.StateMachine.ResetTo(GameState.MainMenu);
            _ui = UiRoot.Create("MainMenuUI", 0);
            var screen = _ui.gameObject.AddComponent<MainMenuScreen>();
            screen.Initialize(_app, this);
        }

        public void ShowLevelSelect()
        {
            Cleanup();
            _currentLevel = null;
            _app.StateMachine.ResetTo(GameState.LevelSelect);
            _ui = UiRoot.Create("LevelSelectUI", 0);
            var screen = _ui.gameObject.AddComponent<LevelSelectScreen>();
            screen.Initialize(_app, this);
        }

        public void StartLevelById(int levelId)
        {
            LevelDefinition level = _app.Levels.GetById(levelId) ?? _app.Levels.GetByIndex(0);
            if (level != null)
            {
                StartLevel(level);
            }
        }

        public void StartLevel(LevelDefinition level)
        {
            if (level == null)
            {
                return;
            }

            Cleanup();
            _currentLevel = level;
            _app.Save.SetLastPlayedLevel(level.Id);
            _app.StateMachine.ResetTo(GameState.Playing);

            _ui = UiRoot.Create("GameplayUI", 100);
            _gameplay = _ui.gameObject.AddComponent<GameplayScreen>();
            _gameplay.Initialize(_app, this, level);
        }

        public void RestartLevel()
        {
            if (_currentLevel != null)
            {
                StartLevel(_currentLevel);
            }
        }

        public void StartNextLevel()
        {
            LevelDefinition next = _currentLevel != null
                ? _app.Levels.GetNext(_currentLevel.Id)
                : null;
            if (next != null)
            {
                StartLevel(next);
            }
            else
            {
                ShowLevelSelect();
            }
        }

        public void PauseForLifecycle()
        {
            _gameplay?.PauseForLifecycle();
        }

        private void Cleanup()
        {
            if (_gameplay != null)
            {
                _gameplay.Close();
                _gameplay = null;
            }

            if (_ui != null)
            {
                Destroy(_ui.gameObject);
                _ui = null;
            }
        }
    }
}
