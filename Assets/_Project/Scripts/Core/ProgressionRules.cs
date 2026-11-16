using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Applies a completed level to progression: best stars, best time and the
    /// sequential unlock rule from the original game.
    /// </summary>
    public static class ProgressionRules
    {
        public static LevelCompletionResult ApplyCompletion(
            ProgressionState state,
            int levelId,
            int levelCount,
            float elapsedSeconds,
            float parTimeSeconds)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            int stars = ScoreCalculator.CalculateStars(elapsedSeconds, parTimeSeconds);
            LevelProgress progress = state.GetOrCreate(levelId);

            bool newBest = !progress.Completed ||
                           progress.BestTime <= 0f ||
                           elapsedSeconds < progress.BestTime;

            progress.Completed = true;
            progress.Stars = Math.Max(progress.Stars, stars);
            if (newBest)
            {
                progress.BestTime = elapsedSeconds;
            }

            bool unlocked = false;
            int nextLevelId = levelId + 1;
            if (levelId >= state.UnlockedLevel && levelId < levelCount)
            {
                state.UnlockedLevel = levelId + 1;
                unlocked = true;
            }

            state.ClampToLevelCount(levelCount);

            return new LevelCompletionResult
            {
                LevelId = levelId,
                Stars = stars,
                ElapsedTime = elapsedSeconds,
                BestTime = progress.BestTime,
                NewBestTime = newBest,
                UnlockedNextLevel = unlocked,
                NextLevelId = nextLevelId
            };
        }
    }
}
