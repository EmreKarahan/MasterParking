using UnityEngine;

namespace MasterParking.SaveSystem
{
    /// <summary>
    /// Small alternative backend for platforms where the file system is
    /// restricted. Kept intentionally tiny; gameplay only sees ISaveStorage.
    /// </summary>
    public sealed class PlayerPrefsSaveStorage : ISaveStorage
    {
        private readonly string _key;

        public PlayerPrefsSaveStorage(string key = "MasterParking.Save")
        {
            _key = key;
        }

        public string Description => "PlayerPrefs:" + _key;

        public string Load() => PlayerPrefs.HasKey(_key) ? PlayerPrefs.GetString(_key) : null;

        public bool Save(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            PlayerPrefs.SetString(_key, payload);
            PlayerPrefs.Save();
            return true;
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(_key);
            PlayerPrefs.Save();
        }
    }
}
