using System;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Analytics
{
    [Serializable]
    public sealed class AnalyticsParameterData
    {
        [SerializeField] private int _id;
        [SerializeField] private EAnalyticsValueType _type;
        [SerializeField] private string _text;
        [SerializeField] private long _integer;
        [SerializeField] private double _number;
        [SerializeField] private bool _boolean;
        public int Id => _id;
        public EAnalyticsValueType Type => _type;
        public string Text => _text;
        public long Integer => _integer;
        public double Number => _number;
        public bool Boolean => _boolean;
        public AnalyticsParameterData(AnalyticsParameterIdStruct id, string value) { _id = id.Value; _type = EAnalyticsValueType.Text; _text = value ?? string.Empty; }
        public AnalyticsParameterData(AnalyticsParameterIdStruct id, long value) { _id = id.Value; _type = EAnalyticsValueType.Integer; _integer = value; }
        public AnalyticsParameterData(AnalyticsParameterIdStruct id, double value) { _id = id.Value; _type = EAnalyticsValueType.Number; _number = value; }
        public AnalyticsParameterData(AnalyticsParameterIdStruct id, bool value) { _id = id.Value; _type = EAnalyticsValueType.Boolean; _boolean = value; }
        public void Validate()
        { if (_id <= 0 || !Enum.IsDefined(typeof(EAnalyticsValueType), _type) || double.IsNaN(_number) || double.IsInfinity(_number)) throw new ArgumentException("Invalid analytics parameter."); }
    }
}
