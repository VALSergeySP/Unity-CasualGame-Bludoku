namespace GameBrewStudio.CoreTemplate.Analytics
{
    // Save must either commit the entire payload or throw without changing the previous payload.
    public interface IAnalyticsStorageService
    {
        string Load();
        void Save(string payload);
    }
}