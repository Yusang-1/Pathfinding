using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitFactory
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        private Action<ISelectableUnit> onSelectedCallbackHandler;
        private Action<ISelectableUnit> onDeselectedCallbackHandler;

        private readonly UnitRuntimeContext unitRuntimeContext;
        private readonly UnitContainerSO unitContainerSO;
        
        private readonly UnitBottomSelectChanger unitBottomPrefab;
        
        private readonly ObjectPool<UnitBottomSelectChanger> unitBottomPool = new();

        public UnitFactory(UnitBottomSelectChanger unitBottomPrefab,UnitRuntimeContext unitRuntimeContext,
            UnitContainerSO unitContainerSO)
        {
            this.unitBottomPrefab = unitBottomPrefab;
            this.unitRuntimeContext = unitRuntimeContext;
            this.unitContainerSO = unitContainerSO;

            InitializeHandlers();
        }

        private void InitializeHandlers()
        {
            onSelectedCallbackHandler = (s) => OnSelectedCallback?.Invoke(s);
            onDeselectedCallbackHandler = (s) => OnDeselectedCallback?.Invoke(s);
        }

        public void SpawnUnit(int unitCode, Vector3 spawnPosition)
        {
            Unit unit = GetUnitInstance(unitCode);
            unit.transform.position = spawnPosition;

            BoundUnitEvent(unit);

            var unitBottom = GetUnitBottomInstance();
            unitBottom.transform.position = spawnPosition;

            unit.Initialize(unitRuntimeContext, unitBottom);
            unit.UnitSpawned();
        }

        private Unit GetUnitInstance(int unitCode)
        {
            if(unitContainerSO.TryGetUnit(unitCode, out Unit unit))
            {
                return unit;
            }
            else
            {
                Debug.LogWarning("Unit Instance를 가져오지 못함");
                return null;
            }            
        }

        private UnitBottomSelectChanger GetUnitBottomInstance()
        {
            if (!unitBottomPool.TryGetObject(out UnitBottomSelectChanger unitBottom))
            {
                // 유닛을 가져오지 못한 경우
                unitBottom = UnitBottomSelectChanger.Instantiate(unitBottomPrefab);
                unitBottom.OnPoolObjectUnused += unitBottomPool.PoolObjectUnused;
            }

            return unitBottom;
        }

        private void BoundUnitEvent(ISelectableUnit unit)
        {
            if ((unit as Unit).IsEventBound) return;

            unit.OnSelectedCallback += onSelectedCallbackHandler;
            unit.OnDeselectedCallback += onDeselectedCallbackHandler;
            (unit as Unit).OnDespawnedCallback += UnboundUnitEvent;
            (unit as Unit).IsEventBound = true;
        }

        private void UnboundUnitEvent(ISelectableUnit unit)
        {
            if (!(unit as Unit).IsEventBound) return;

            unit.OnSelectedCallback -= onSelectedCallbackHandler;
            unit.OnDeselectedCallback -= onDeselectedCallbackHandler;
            (unit as Unit).OnDespawnedCallback -= UnboundUnitEvent;
            (unit as Unit).IsEventBound = false;
        }
    }
}
