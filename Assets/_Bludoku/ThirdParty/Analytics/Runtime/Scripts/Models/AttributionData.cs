using System;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AttributionData
    {
        [SerializeField] private string _source;
        [SerializeField] private string _campaign;
        [SerializeField] private string _adGroup;
        [SerializeField] private string _creative;
        public string Source => _source;
        public string Campaign => _campaign;
        public string AdGroup => _adGroup;
        public string Creative => _creative;
        public AttributionData(string source, string campaign, string adGroup, string creative)
        { _source = source; _campaign = campaign; _adGroup = adGroup; _creative = creative; }
    }
}
