using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AnalyticsQueueEntryData
    {
        [SerializeField] private AnalyticsEventData _event;
        [SerializeField] private List<AnalyticsDeliveryData> _deliveries;
        public AnalyticsEventData Event => _event;
        public IReadOnlyList<AnalyticsDeliveryData> Deliveries => _deliveries.AsReadOnly();
        internal List<AnalyticsDeliveryData> MutableDeliveries => _deliveries;
        internal AnalyticsQueueEntryData(AnalyticsEventData data, List<AnalyticsDeliveryData> deliveries) { _event = data; _deliveries = deliveries; }
        internal void Validate()
        {
            _event.Validate(); if (_deliveries == null) throw new ArgumentException("Missing analytics deliveries.");
            var ids = new HashSet<int>();
            foreach (AnalyticsDeliveryData delivery in _deliveries)
                if (delivery == null || delivery.ProviderId <= 0 || !ids.Add(delivery.ProviderId) || delivery.Attempts < 0 || delivery.NextUtcTicks < 0)
                    throw new ArgumentException("Invalid analytics delivery.");
        }
    }
}
