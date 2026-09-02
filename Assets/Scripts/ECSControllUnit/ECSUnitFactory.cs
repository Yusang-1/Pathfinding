using UnityEngine;
using Unity.Entities;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitFactory
    {
        private readonly UnitRuntimeContext unitRuntimeContext;
        private readonly EntityManager entityManager;

        public ECSUnitFactory(UnitRuntimeContext unitRuntimeContext)
        {
            this.unitRuntimeContext = unitRuntimeContext;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        public void SpawnUnit(UnitSize unitSize, Vector3 spawnPosition)
        {
            Entity requestEntity = GetUnitInstance(unitSize, spawnPosition);
            
            // var name = entityManager.GetComponentData<ECSUnitComponent>(requestEntity).UnitName;
            // unitRuntimeContext.DataContainer.Register(requestEntity, name);

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
    }
}

