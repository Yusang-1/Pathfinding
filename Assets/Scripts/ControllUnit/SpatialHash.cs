using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit
{
    public class SpatialHash
    {
        private readonly int cellSize = 2;
        private readonly Dictionary<Vector2Int, List<Unit>> hashTable = new();

        // 해시 키 생성
        private Vector2Int GetHashKey(Vector3 pos) => new((int)(pos.x / cellSize), (int)(pos.y / cellSize));

        // 객체 추가
        public void AddUnit(Unit unit)
        {
            Vector2Int key = GetHashKey(unit.transform.position);
            if (!hashTable.ContainsKey(key)) hashTable[key] = new List<Unit>();
            hashTable[key].Add(unit);
            unit.CurrentKey = key;
        }

        public void RemoveUnit(Unit unit)
        {
            if (!hashTable.ContainsKey(unit.CurrentKey))
            {
                Debug.LogWarning("unit이 존재하지 않는 SpatialHash 키를 가지고 있음");
                return;
            }

            hashTable[unit.CurrentKey].Remove(unit);
        }

        public void CheckUnitHash(Unit unit)
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
        public List<Unit> GetUnitsInRange(Vector3 center, float radius)
        {
            List<Unit> result = new List<Unit>();
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
        public List<ISelectableUnit> GetUnitsInRange(Vector3 standard, float width, float height)
        {
            Rect screenRect = new Rect(
                Mathf.Min(standard.x, standard.x + width),
                Mathf.Min(standard.y, standard.y + height),
                Mathf.Abs(width),
                Mathf.Abs(height)
            );

            Camera cam = Camera.main;
            if (cam == null)
                return new List<ISelectableUnit>();

            float depth = cam.orthographic ? 0f : Mathf.Abs(cam.transform.position.z);
            Vector3 worldBottomLeft = cam.ScreenToWorldPoint(new Vector3(screenRect.xMin, screenRect.yMin, depth));
            Vector3 worldTopRight = cam.ScreenToWorldPoint(new Vector3(screenRect.xMax, screenRect.yMax, depth));

            Vector2Int minKey = GetHashKey(new Vector3(Mathf.Min(worldBottomLeft.x, worldTopRight.x), Mathf.Min(worldBottomLeft.y, worldTopRight.y)));
            Vector2Int maxKey = GetHashKey(new Vector3(Mathf.Max(worldBottomLeft.x, worldTopRight.x), Mathf.Max(worldBottomLeft.y, worldTopRight.y)));

            List<ISelectableUnit> result = new List<ISelectableUnit>();

            for (int x = minKey.x; x <= maxKey.x; x++)
            {
                for (int y = minKey.y; y <= maxKey.y; y++)
                {
                    Vector2Int key = new Vector2Int(x, y);
                    if (!hashTable.TryGetValue(key, out List<Unit> bucket))
                        continue;

                    foreach (Unit unit in bucket)
                    {
                        if (!(unit is ISelectableUnit selectable))
                            continue;

                        Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);
                        if (screenPos.z < 0)
                            continue;

                        if (screenRect.Contains(screenPos))
                        {
                            result.Add(selectable);
                        }
                    }
                }
            }

            return result;
        }
    }
}
