using System;
using Assets.Scripts.ControllUnit;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitFactory
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        private Action<ISelectableUnit> onSelectedCallbackHandler;
        private Action<ISelectableUnit> onDeselectedCallbackHandler;

        private readonly UnitRuntimeContext unitRuntimeContext;

        public ECSUnitFactory(UnitRuntimeContext unitRuntimeContext)
        {
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
            Entity requestEntity = GetUnitInstance(unitSize, spawnPosition);

            // Unit unit = GetUnitInstance(unitSize);
            // unit.transform.position = spawnPosition;

            // BoundUnitEvent(unit);

            // var unitBottom = GetUnitBottomInstance();
            // unitBottom.transform.position = spawnPosition;

            // unit.Initialize(unitRuntimeContext, unitBottom);
            // unit.UnitSpawned();
        }

        private Entity GetUnitInstance(UnitSize unitSize, Vector3 spawnPosition)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity requestEntity = entityManager.CreateEntity();

            entityManager.AddComponentData(
                requestEntity,
                new UnitSpawnRequestComponent()
                { 
                    UnitSize = unitSize,
                    Position = spawnPosition
                }
            );

            return requestEntity;
        }

        private UnitBottomSelectChanger GetUnitBottomInstance()
        {
            return null;
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

