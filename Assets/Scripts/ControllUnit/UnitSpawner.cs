using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class UnitSpawner : MonoBehaviour
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        
        [SerializeField] private Unit unitPrefab;
        [SerializeField] private UnitBottomSelectChanger unitBottomPrefab;
        [SerializeField] private Vector3 spawnPosition;
        
        private SpatialHash spatialHash;
        private readonly ObjectPool<Unit> unitPool = new();
        private readonly ObjectPool<UnitBottomSelectChanger> unitBottomPool = new();
        
        public void Initialize(SpatialHash spatialHash)
        {
            this.spatialHash = spatialHash;
        }
        
        public void SpawnUnit()
        {
            if (!unitPool.TryGetObject(out Unit unit))
            {
                // 유닛을 가져오지 못한 경우
                unit = Instantiate(unitPrefab);
                unit.OnPoolObjectFirstCreated += unitPool.PoolObjectFirstCreated;
                unit.OnPoolObjectUnused += unitPool.PoolObjectUnused;
                                
                if(unit is ISelectableUnit)
                {
                    unit.OnSelectedCallback += (s) => OnSelectedCallback?.Invoke(s);
                    unit.OnDeselectedCallback += (s) => OnDeselectedCallback?.Invoke(s);
                }
            }
            unit.transform.position = spawnPosition;
            
            
            if (!unitBottomPool.TryGetObject(out UnitBottomSelectChanger unitBottom))
            {
                // 유닛을 가져오지 못한 경우
                unitBottom = Instantiate(unitBottomPrefab);
                unitBottom.OnPoolObjectFirstCreated += unitBottomPool.PoolObjectFirstCreated;
                unitBottom.OnPoolObjectUnused += unitBottomPool.PoolObjectUnused;                            
            }
            unitBottom.transform.position = spawnPosition;            
            
            unit.Initialize(spatialHash, unitBottom);            
            unit.UnitSpawned();
        }
    }
}
