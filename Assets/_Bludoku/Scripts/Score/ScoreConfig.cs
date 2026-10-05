using UnityEngine;

namespace _Bludoku.Scripts.Score
{
    public enum EScoreRounding { Floor = 0, Nearest = 1, Ceiling = 2 }

    [CreateAssetMenu(menuName = "Bludoku/Score Config")]
    public sealed class ScoreConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _pointsPerCell = 1f;
        [SerializeField] private EScoreRounding _rounding = EScoreRounding.Floor;

        public int Calculate(int clearedCells, float generalMultiplier, float destructionMultiplier)
        {
            float points = clearedCells * _pointsPerCell * generalMultiplier * destructionMultiplier;
            switch (_rounding)
            {
                case EScoreRounding.Nearest: return Mathf.RoundToInt(points);
                case EScoreRounding.Ceiling: return Mathf.CeilToInt(points);
                default: return Mathf.FloorToInt(points);
            }
        }
    }
}
