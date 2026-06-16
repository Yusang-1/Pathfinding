using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField] private Unit unitPrefab;
        [SerializeField] private Vector3 spawnPosition;

        private readonly ObjectPool<Unit> unitPool = new();

        public void SpawnUnit()
        {
            if (!unitPool.TryGetObject(out Unit unit))
            {
                // 유닛을 가져오지 못한 경우
                unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                unit.OnPoolObjectFirstCreated += unitPool.PoolObjectFirstCreated;
                unit.OnPoolObjectUnused += unitPool.PoolObjectUnused;
            }
            else
            {
                unit.transform.position = spawnPosition;
            }

            unit.UnitSpawned();
        }
    }
}
