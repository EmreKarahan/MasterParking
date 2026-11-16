namespace MasterParking.Core
{
    /// <summary>Star rules from the source game: par, then 145% of par.</summary>
    public static class ScoreCalculator
    {
        public const int MaxStars = 3;

        public static int CalculateStars(float elapsedSeconds, float parTimeSeconds)
        {
            if (parTimeSeconds <= 0f)
            {
                return MaxStars;
            }

            if (elapsedSeconds <= parTimeSeconds)
            {
                return 3;
            }

            if (elapsedSeconds <= parTimeSeconds * 1.45f)
            {
                return 2;
            }

            return 1;
        }
    }
}
