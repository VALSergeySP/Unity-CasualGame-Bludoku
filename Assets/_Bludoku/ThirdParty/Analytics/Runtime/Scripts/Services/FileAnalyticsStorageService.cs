using System;
using System.IO;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public sealed class FileAnalyticsStorageService : IAnalyticsStorageService
    {
        private const string TemporaryExtension = ".tmp";
        private readonly string _path;
        public FileAnalyticsStorageService(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Missing storage path.", nameof(path));
            _path = Path.GetFullPath(path);
        }
        public string Load() => File.Exists(_path) ? File.ReadAllText(_path) : null;
        public void Save(string payload)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            string temporaryPath = _path + TemporaryExtension;
            try
            {
                File.WriteAllText(temporaryPath, payload);
                if (File.Exists(_path)) File.Replace(temporaryPath, _path, null);
                else File.Move(temporaryPath, _path);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }
    }
}