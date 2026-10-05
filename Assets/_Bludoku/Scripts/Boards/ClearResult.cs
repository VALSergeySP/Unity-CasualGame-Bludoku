using System.Collections.Generic;
using UnityEngine;

namespace _Bludoku.Scripts.Boards
{
    public class ClearResult
    {
        public int PieceId { get; internal set; }
        public int Column { get; internal set; }
        public int Row { get; internal set; }
        public int ClearedCount;
        public int FiguresRemovedCount;
        public List<Vector3> ClearedPositions;
        public Vector3 PlacementPosition { get; internal set; }
    }
}
