using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AnalyticsEventData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _session;
        [SerializeField] private long _sequence;
        [SerializeField] private long _utcTicks;
        [SerializeField] private string _version;
        [SerializeField] private string _platform;
        [SerializeField] private string _language;
        [SerializeField] private EAnalyticsEvent _kind;
        [SerializeField] private int _customId;
        [SerializeField] private EAnalyticsGroup _group;
        [SerializeField] private List<AnalyticsParameterData> _parameters;
        public Guid Id => Guid.Parse(_id);
        public Guid Session => Guid.Parse(_session);
        public long Sequence => _sequence;
        public long UtcTicks => _utcTicks;
        public string Version => _version;
        public string Platform => _platform;
        public string Language => _language;
        public EAnalyticsEvent Kind => _kind;
        public int CustomId => _customId;
        public EAnalyticsGroup Group => _group;
        public IReadOnlyList<AnalyticsParameterData> Parameters => _parameters.AsReadOnly();
        internal AnalyticsEventData(Guid session, long sequence, DateTime now, string version, string platform, string language,
            EAnalyticsEvent kind, EAnalyticsGroup group, IEnumerable<AnalyticsParameterData> parameters, int customId)
        {
            _id = Guid.NewGuid().ToString("N"); _session = session.ToString("N"); _sequence = sequence; _utcTicks = now.Ticks;
            _version = version; _platform = platform; _language = language; _kind = kind; _group = group; _customId = customId;
            _parameters = new List<AnalyticsParameterData>(parameters ?? Array.Empty<AnalyticsParameterData>()); Validate();
        }
        public void Validate()
        {
            if (Id == Guid.Empty || Session == Guid.Empty || _sequence <= 0 || _utcTicks <= 0 || _utcTicks > DateTime.MaxValue.Ticks ||
                !Enum.IsDefined(typeof(EAnalyticsEvent), _kind) || _group == EAnalyticsGroup.None || (_group & ~EAnalyticsGroup.All) != 0 ||
                (_kind == EAnalyticsEvent.Custom && _customId <= 0) || _parameters == null) throw new ArgumentException("Invalid analytics event.");
            var ids = new HashSet<int>();
            foreach (AnalyticsParameterData parameter in _parameters)
            { if (parameter == null || !ids.Add(parameter.Id)) throw new ArgumentException("Duplicate or missing analytics parameter."); parameter.Validate(); }
        }
    }
}
