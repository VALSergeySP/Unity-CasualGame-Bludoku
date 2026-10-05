using System;

namespace GameBrewStudio.CoreTemplate.Analytics
{
    public readonly struct AnalyticsProviderIdStruct : IEquatable<AnalyticsProviderIdStruct>
    {
        public int Value { get; }
        public AnalyticsProviderIdStruct(int value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }
        public bool Equals(AnalyticsProviderIdStruct other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AnalyticsProviderIdStruct other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }
}