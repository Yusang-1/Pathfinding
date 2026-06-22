using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.CrowdSimulation
{
    public class SpatialHash
    {
        private readonly int cellSize = 2;
        private readonly Dictionary<Vector2Int, List<CrowdUnit>> hashTable = new();
        private readonly List<CrowdUnit> result = new();

        // 해시 키 생성
        private Vector2Int GetHashKey(Vector3 pos) => new((int)(pos.x / cellSize), (int)(pos.y / cellSize));

        // 객체 추가
        public void AddUnit(CrowdUnit unit)
        {
            Vector2Int key = GetHashKey(unit.transform.position);
            if (!hashTable.ContainsKey(key)) hashTable[key] = new List<CrowdUnit>();
            hashTable[key].Add(unit);
            unit.CurrentKey = key;
        }

        public void RemoveUnit(CrowdUnit unit)
        {
            if (!hashTable.ContainsKey(unit.CurrentKey))
            {
                Debug.LogWarning("unit이 존재하지 않는 SpatialHash 키를 가지고 있음");
                return;
            }

            hashTable[unit.CurrentKey].Remove(unit);
        }

        public void CheckUnitHash(CrowdUnit unit)
        {
            Vector2Int key = GetHashKey(unit.transform.position);
            if (unit.CurrentKey != key)
            {
                Debug.Log($"unit의 hash변경 {unit.CurrentKey} -> {key}");
                RemoveUnit(unit);
                AddUnit(unit);
            }
        }

        // 주변 해시 검색
        public List<CrowdUnit> GetUnitsInRange(Vector3 center, float radius)
        {
            result.Clear();
            Vector2Int centerKey = GetHashKey(center);
            int range = Mathf.CeilToInt(radius / cellSize);

            for (int x = centerKey.x - range; x <= centerKey.x + range; x++)
            {
                for (int y = centerKey.y - range; y <= centerKey.y + range; y++)
                {
                    Vector2Int key = new Vector2Int(x, y);
                    if (hashTable.ContainsKey(key))
                    {
                        result.AddRange(hashTable[key]);
                    }
                }
            }
            return result;
        }
    }
}
