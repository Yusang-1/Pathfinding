using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class UnitFactory
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        private Action<ISelectableUnit> onSelectedCallbackHandler;
        private Action<ISelectableUnit> onDeselectedCallbackHandler;

        private readonly Unit smallUnitPrefab;
        private readonly Unit largeUnitPrefab;
        private readonly UnitBottomSelectChanger unitBottomPrefab;

        private readonly UnitRuntimeContext unitRuntimeContext;
        private readonly ObjectPool<Unit> smallUnitPool = new();
        private readonly ObjectPool<Unit> largeUnitPool = new();
        private readonly ObjectPool<UnitBottomSelectChanger> unitBottomPool = new();

        public UnitFactory(Unit smallUnitPrefab, Unit largeUnitPrefab, UnitBottomSelectChanger unitBottomPrefab, Pathfinder pathfinder, UnitRuntimeContext unitRuntimeContext)
        {
            this.smallUnitPrefab = smallUnitPrefab;
            this.largeUnitPrefab = largeUnitPrefab;
            this.unitBottomPrefab = unitBottomPrefab;
            this.unitRuntimeContext = unitRuntimeContext;

            InitializeHandlers();
        }

        private void InitializeHandlers()
        {
            onSelectedCallbackHandler = (s) => OnSelectedCallback?.Invoke(s);
            onDeselectedCallbackHandler = (s) => OnDeselectedCallback?.Invoke(s);
        }

        public void SpawnUnit(UnitSize unitSize, Vector3 spawnPosition)
        {
            Unit unit = GetUnitInstance(unitSize);
            unit.transform.position = spawnPosition;

            BoundUnitEvent(unit);

            var unitBottom = GetUnitBottomInstance();
            unitBottom.transform.position = spawnPosition;

            unit.Initialize(unitRuntimeContext, unitBottom);
            unit.UnitSpawned();
        }

        private Unit GetUnitInstance(UnitSize unitSize)
        {
            var unitPool = unitSize == UnitSize.small ? smallUnitPool : largeUnitPool;
            var unitPrefab = unitSize == UnitSize.small ? smallUnitPrefab : largeUnitPrefab;

            if (!unitPool.TryGetObject(out Unit unit))
            {
                // 유닛을 가져오지 못한 경우
                unit = Unit.Instantiate(unitPrefab);
                unit.OnPoolObjectFirstCreated += unitPool.PoolObjectFirstCreated;
                unit.OnPoolObjectUnused += unitPool.PoolObjectUnused;
            }

            return unit;
        }

        private UnitBottomSelectChanger GetUnitBottomInstance()
        {
            if (!unitBottomPool.TryGetObject(out UnitBottomSelectChanger unitBottom))
            {
                // 유닛을 가져오지 못한 경우
                unitBottom = UnitBottomSelectChanger.Instantiate(unitBottomPrefab);
                unitBottom.OnPoolObjectFirstCreated += unitBottomPool.PoolObjectFirstCreated;
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
