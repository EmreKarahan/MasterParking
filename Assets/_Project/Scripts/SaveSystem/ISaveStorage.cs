namespace MasterParking.SaveSystem
{
    /// <summary>
    /// Persistence seam. Swap the file implementation for cloud saves, platform
    /// key-value stores or tests without touching gameplay code.
    /// </summary>
    public interface ISaveStorage
    {
        string Load();
        bool Save(string payload);
        void Delete();

        /// <summary>Human-readable location, useful for diagnostics.</summary>
        string Description { get; }
    }
}
