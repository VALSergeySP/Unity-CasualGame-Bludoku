using GameBrewStudio.CoreTemplate.Analytics;
using UnityEditor;
using UnityEngine;
namespace GameBrewStudio.CoreTemplate.Samples.Editor
{
    public sealed class AnalyticsEventsWindow : EditorWindow
    {
        private const float DetailHeight = 210f;
        private const float DetailPadding = 40f;
        private Vector2 _scroll;
        private Vector2 _detailScroll;
        private AnalyticsEventData _selected;
        private EAnalyticsGroup _groups = EAnalyticsGroup.All;
        private bool _showQueue;
        [MenuItem("GameBrewStudio/Core Template/Analytics Events")]
        public static void Open() { GetWindow<AnalyticsEventsWindow>("Analytics Events"); }
        private void OnInspectorUpdate() { Repaint(); }
        private void OnGUI()
        {
            SampleAnalyticsModuleSystem module = SampleAnalyticsModuleSystem.Active;
            if (module?.System == null) { EditorGUILayout.HelpBox("Initialize SampleAnalyticsModuleSystem from your host to inspect analytics in Play Mode.", MessageType.Info); return; }
            EditorGUILayout.LabelField("Local simulated analytics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Session", module.System.SessionId.ToString());
            EditorGUILayout.LabelField("Pending events", module.System.Queue.Count.ToString());
            EditorGUILayout.LabelField("Attribution", module.AttributionStatus);
            foreach (AnalyticsRouteConfig route in module.Config.Routes)
            {
                SampleAnalyticsProvider provider = module.GetProvider(route.Provider.Id.Value);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(route.Provider.name + " / " + route.Groups);
                    if (provider == null) EditorGUILayout.LabelField("Initializing / unavailable");
                    else provider.Response = (ESampleAnalyticsResponse)EditorGUILayout.EnumPopup(provider.Response);
                }
            }
            _groups = (EAnalyticsGroup)EditorGUILayout.EnumFlagsField("Event groups", _groups);
            _showQueue = GUILayout.Toolbar(_showQueue ? 1 : 0, new[] { "Delivery history", "Persistent queue" }) == 1;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_showQueue)
            {
                foreach (AnalyticsQueueEntryData entry in module.System.Queue)
                {
                    if ((entry.Event.Group & _groups) == 0) continue;
                    if (GUILayout.Button(entry.Event.Sequence + " · " + entry.Event.Kind + " · " + entry.Event.Id, EditorStyles.miniButton)) _selected = entry.Event;
                    foreach (AnalyticsDeliveryData delivery in entry.Deliveries)
                        EditorGUILayout.LabelField("Provider " + delivery.ProviderId + " · attempts " + delivery.Attempts + " · retry UTC " + new System.DateTime(delivery.NextUtcTicks).ToString("O"));
                }
            }
            else
            {
                foreach (AnalyticsDiagnosticModel row in module.System.History)
                {
                    if (row.Event != null && (row.Event.Group & _groups) == 0) continue;
                    if (GUILayout.Button((row.Event?.Sequence.ToString() ?? "—") + " · " + row.Event?.Kind + " · provider " + row.ProviderId + " · " + row.Status, EditorStyles.miniButton)) _selected = row.Event;
                    if (!string.IsNullOrEmpty(row.Detail)) EditorGUILayout.LabelField(row.Detail, EditorStyles.wordWrappedMiniLabel);
                }
            }
            EditorGUILayout.EndScrollView();
            if (_selected == null) return;
            EditorGUILayout.LabelField("Selected event and typed parameters", EditorStyles.boldLabel);
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll, GUILayout.Height(DetailHeight));
            foreach (AnalyticsParameterData parameter in _selected.Parameters)
            {
                string name = System.Enum.IsDefined(typeof(EAnalyticsParameter), parameter.Id) ? ((EAnalyticsParameter)parameter.Id).ToString() : "Custom " + parameter.Id;
                string value = parameter.Type == EAnalyticsValueType.Text ? parameter.Text : parameter.Type == EAnalyticsValueType.Integer ? parameter.Integer.ToString() :
                    parameter.Type == EAnalyticsValueType.Number ? parameter.Number.ToString(System.Globalization.CultureInfo.InvariantCulture) : parameter.Boolean.ToString();
                EditorGUILayout.LabelField(name + " (" + parameter.Type + ")", value, EditorStyles.wordWrappedLabel);
            }
            string json = JsonUtility.ToJson(_selected, true);
            float height = EditorStyles.textArea.CalcHeight(new GUIContent(json), Mathf.Max(1f, position.width - DetailPadding));
            EditorGUILayout.SelectableLabel(json, EditorStyles.textArea, GUILayout.Height(Mathf.Max(DetailHeight, height)));
            EditorGUILayout.EndScrollView();
        }
    }
}
