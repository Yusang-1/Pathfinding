using UnityEngine;

namespace Assets.Scripts.CreateMap
{

    public class MapData
    {
        public Info InfoData { get; private set; }
        public Terrain TerrainData { get; private set; }
        public Unit UnitData { get; private set; }

        public MapData(Info info, Terrain terrain, Unit unit)
        {
            InfoData = info;
            TerrainData = terrain;
            UnitData = unit;
        }

        public struct Info
        {
            public int MapCode;
            public string MapName;
            public int MapSize;
        }

        public struct Terrain
        {
            public int MapSize;
            public Vector2Int[] ObstacleIndexes;
        }

        public struct Unit
        {
            public int[] UnitCodes;
            public float[] PosX;
            public float[] PosY;
            public float[] PosZ;

            // MapData도 그렇고 배열을 사용하게 되는데 이럼 힙에 저장되지 않나 이거 물어봐야겠다
        }
    }
}
