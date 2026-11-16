using MasterParking.Core;

namespace MasterParking.Gameplay
{
    /// <summary>Crash copy preserved from the source game.</summary>
    public static class CrashMessages
    {
        public const string OutOfBounds = "Alan dışına çıktın!";

        public static string For(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.Wall:
                    return "Bariyere çarptın!";
                case ObstacleType.ParkedCar:
                    return "Park halindeki bir araca çarptın!";
                case ObstacleType.MovingCar:
                    return "Hareket halindeki araca çarptın!";
                case ObstacleType.Cone:
                    return "Trafik dubasına çarptın!";
                default:
                    return "Kaza yaptın!";
            }
        }

        public static string ForOutOfBounds() => OutOfBounds;
    }
}
