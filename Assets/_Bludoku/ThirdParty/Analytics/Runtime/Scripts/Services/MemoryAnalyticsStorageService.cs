namespace GameBrewStudio.CoreTemplate.Analytics
{
    public sealed class MemoryAnalyticsStorageService : IAnalyticsStorageService
    {
        private string _payload;
        public string Load() => _payload;
        public void Save(string payload) => _payload = payload;
    }
}