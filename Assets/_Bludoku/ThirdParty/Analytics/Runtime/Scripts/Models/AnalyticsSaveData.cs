using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AnalyticsSaveData
    {
        [SerializeField] private long _sequence;
        [SerializeField] private List<AnalyticsQueueEntryData> _queue = new List<AnalyticsQueueEntryData>();
        public long Sequence { get => _sequence; internal set => _sequence = value; }
        public IReadOnlyList<AnalyticsQueueEntryData> Queue => _queue.AsReadOnly();
        internal List<AnalyticsQueueEntryData> MutableQueue => _queue;
        public void Validate()
        {
            if (_sequence < 0 || _queue == null) throw new ArgumentException("Invalid analytics save.");
            var ids = new HashSet<Guid>(); long previous = 0;
            foreach (AnalyticsQueueEntryData entry in _queue)
            {
                if (entry == null) throw new ArgumentException("Missing analytics entry."); entry.Validate();
                if (!ids.Add(entry.Event.Id) || entry.Event.Sequence <= previous || entry.Event.Sequence > _sequence) throw new ArgumentException("Invalid analytics order.");
                previous = entry.Event.Sequence;
            }
        }
    }
}
