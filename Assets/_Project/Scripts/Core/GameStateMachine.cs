using System;
using System.Collections.Generic;

namespace MasterParking.Core
{
    /// <summary>Central game flow. UI and systems react to changes instead of booleans.</summary>
    public enum GameState
    {
        Boot = 0,
        MainMenu = 1,
        LevelSelect = 2,
        Playing = 3,
        Parking = 4,
        Crashed = 5,
        LevelCompleted = 6,
        Paused = 7
    }

    /// <summary>
    /// Guards legal state transitions and publishes them. Parking is a sub-state of
    /// active play; Crashed/Completed/Paused are terminal until the player acts.
    /// </summary>
    public sealed class GameStateMachine
    {
        private static readonly Dictionary<GameState, GameState[]> Allowed = new Dictionary<GameState, GameState[]>
        {
            {
                GameState.Boot, new[] { GameState.MainMenu, GameState.LevelSelect, GameState.Playing }
            },
            {
                GameState.MainMenu, new[] { GameState.LevelSelect, GameState.Playing, GameState.MainMenu }
            },
            {
                GameState.LevelSelect, new[] { GameState.Playing, GameState.MainMenu }
            },
            {
                GameState.Playing, new[]
                {
                    GameState.Parking, GameState.Crashed, GameState.LevelCompleted, GameState.Paused,
                    GameState.LevelSelect, GameState.MainMenu
                }
            },
            {
                GameState.Parking, new[]
                {
                    GameState.Playing, GameState.Crashed, GameState.LevelCompleted, GameState.Paused,
                    GameState.LevelSelect, GameState.MainMenu
                }
            },
            {
                GameState.Crashed, new[]
                {
                    GameState.Playing, GameState.LevelSelect, GameState.MainMenu, GameState.Crashed
                }
            },
            {
                GameState.LevelCompleted, new[]
                {
                    GameState.Playing, GameState.LevelSelect, GameState.MainMenu, GameState.LevelCompleted
                }
            },
            {
                GameState.Paused, new[]
                {
                    GameState.Playing, GameState.Parking, GameState.LevelSelect, GameState.MainMenu
                }
            }
        };

        public GameState State { get; private set; } = GameState.Boot;
        public event Action<GameState, GameState> StateChanged;

        public bool CanTransitionTo(GameState next)
        {
            if (next == State)
            {
                return true;
            }

            return Allowed.TryGetValue(State, out GameState[] targets) && Array.IndexOf(targets, next) >= 0;
        }

        public bool TryTransition(GameState next)
        {
            if (next == State)
            {
                return false;
            }

            if (!CanTransitionTo(next))
            {
                return false;
            }

            Transition(next);
            return true;
        }

        /// <summary>Used by restart/load, which intentionally resets flow to a level start.</summary>
        public void ResetTo(GameState next)
        {
            Transition(next);
        }

        private void Transition(GameState next)
        {
            GameState previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }

        public bool IsPlaying => State == GameState.Playing || State == GameState.Parking;
        public bool IsInteractable => IsPlaying;
    }
}
