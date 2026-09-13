using UnityEngine;
using System;

namespace Assets.Scripts.CreateMap
{
    [Serializable]
    public struct MapData
    {
        public string MapName;
        public int NodeSize;
        public int MapSize;
        public int ClusterSize;
        public Vector2Int[] ObstacleIndexes;
    }

    [Serializable]
    public struct MapDataUnitPosition
    {
        public int[] unitCodes;
        public Vector3[] positions;

        // MapData도 그렇고 배열을 사용하게 되는데 이럼 힙에 저장되지 않나 이거 물어봐야겠다
    }
}
