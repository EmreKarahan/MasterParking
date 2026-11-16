namespace MasterParking.Core
{
    /// <summary>Stopwatch for a level attempt. Pausable and engine independent.</summary>
    public sealed class LevelTimer
    {
        public float Elapsed { get; private set; }
        public bool IsRunning { get; private set; }

        public void Reset()
        {
            Elapsed = 0f;
            IsRunning = false;
        }

        public void Start()
        {
            Elapsed = 0f;
            IsRunning = true;
        }

        public void Stop() => IsRunning = false;

        public void Resume() => IsRunning = true;

        public void Tick(float deltaTime)
        {
            if (!IsRunning || deltaTime <= 0f)
            {
                return;
            }

            Elapsed += deltaTime;
        }
    }
}
