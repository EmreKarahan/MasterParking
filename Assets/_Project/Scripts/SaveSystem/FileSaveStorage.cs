using System;
using System.IO;
using UnityEngine;

namespace MasterParking.SaveSystem
{
    /// <summary>
    /// Atomic file storage under Application.persistentDataPath. Writes to a
    /// temporary file first so a terminated app cannot leave a half-written save.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;
        private readonly string _tempPath;

        public FileSaveStorage(string fileName = "masterparking_save.json")
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
            _tempPath = _path + ".tmp";
        }

        public string Description => _path;

        public string Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return null;
                }

                return File.ReadAllText(_path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Save] Could not read save file: " + exception.Message);
                return null;
            }
        }

        public bool Save(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? Application.persistentDataPath);
                File.WriteAllText(_tempPath, payload);

                if (File.Exists(_path))
                {
                    File.Replace(_tempPath, _path, null);
                }
                else
                {
                    File.Move(_tempPath, _path);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Save] Could not write save file: " + exception.Message);
                TryDelete(_tempPath);
                return false;
            }
        }

        public void Delete()
        {
            TryDelete(_path);
            TryDelete(_tempPath);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
                // A failed cleanup must never break gameplay.
            }
        }
    }
}
