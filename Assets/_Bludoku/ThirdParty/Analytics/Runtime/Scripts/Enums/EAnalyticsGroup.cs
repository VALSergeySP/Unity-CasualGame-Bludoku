using System;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Flags]
    public enum EAnalyticsGroup { None = 0, Application = 1, Gameplay = 2, Economy = 4, Monetization = 8, UI = 16, Attribution = 32, Custom = 64, All = 127 }
}
