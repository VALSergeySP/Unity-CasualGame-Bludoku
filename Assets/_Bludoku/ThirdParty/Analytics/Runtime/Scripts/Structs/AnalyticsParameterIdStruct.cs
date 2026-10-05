using System;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    public readonly struct AnalyticsParameterIdStruct
    {
        public const int FirstCustomId = 1000;
        public int Value { get; }
        public AnalyticsParameterIdStruct(EAnalyticsParameter value) { Value = (int)value; }
        private AnalyticsParameterIdStruct(int value) { Value = value; }
        public static AnalyticsParameterIdStruct Custom(int value)
        { if (value < FirstCustomId) throw new ArgumentOutOfRangeException(nameof(value)); return new AnalyticsParameterIdStruct(value); }
    }
}
