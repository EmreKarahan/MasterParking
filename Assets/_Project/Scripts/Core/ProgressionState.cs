using System;
using System.Collections.Generic;

namespace MasterParking.Core
{
    [Serializable]
    public sealed class LevelProgress
    {
        public int LevelId;
        public int Stars;
        public float BestTime;
        public bool Completed;

        public LevelProgress Clone() => new LevelProgress
        {
            LevelId = LevelId,
            Stars = Stars,
            BestTime = BestTime,
            Completed = Completed
        };
    }

    /// <summary>All persistent player progress, serialized by the save layer.</summary>
    [Serializable]
    public sealed class ProgressionState
    {
        /// <summary>Highest level number the player may enter (1-based).</summary>
        public int UnlockedLevel = 1;

        public List<LevelProgress> Levels = new List<LevelProgress>();

        public LevelProgress GetOrCreate(int levelId)
        {
            if (Levels == null)
            {
                Levels = new List<LevelProgress>();
            }

            for (int i = 0; i < Levels.Count; i++)
            {
                if (Levels[i].LevelId == levelId)
                {
                    return Levels[i];
                }
            }

            var progress = new LevelProgress { LevelId = levelId, Stars = 0, BestTime = 0f, Completed = false };
            Levels.Add(progress);
            return progress;
        }

        public bool TryGet(int levelId, out LevelProgress progress)
        {
            if (Levels != null)
            {
                for (int i = 0; i < Levels.Count; i++)
                {
                    if (Levels[i].LevelId == levelId)
                    {
                        progress = Levels[i];
                        return true;
                    }
                }
            }

            progress = null;
            return false;
        }

        public bool IsUnlocked(int levelId) => levelId >= 1 && levelId <= Math.Max(1, UnlockedLevel);

        public void ClampToLevelCount(int levelCount)
        {
            int max = Math.Max(1, levelCount);
            UnlockedLevel = (int)MathUtil.Clamp(UnlockedLevel, 1, max);

            if (Levels == null)
            {
                Levels = new List<LevelProgress>();
                return;
            }

            for (int i = Levels.Count - 1; i >= 0; i--)
            {
                LevelProgress entry = Levels[i];
                if (entry == null || entry.LevelId < 1 || entry.LevelId > max)
                {
                    Levels.RemoveAt(i);
                    continue;
                }

                entry.Stars = (int)MathUtil.Clamp(entry.Stars, 0, ScoreCalculator.MaxStars);
                if (entry.BestTime < 0f)
                {
                    entry.BestTime = 0f;
                }
            }
        }
    }

    /// <summary>Output of a completed level, shared by UI and save logic.</summary>
    public struct LevelCompletionResult
    {
        public int LevelId;
        public int Stars;
        public float ElapsedTime;
        public float BestTime;
        public bool NewBestTime;
        public bool UnlockedNextLevel;
        public int NextLevelId;
    }
}
