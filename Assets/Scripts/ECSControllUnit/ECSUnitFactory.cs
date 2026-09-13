using UnityEngine;
using Unity.Entities;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSUnitFactory
    {
        public void MakeUnitSpawnRequest(int unitCode, Vector3 spawnPosition)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity requestEntity = entityManager.CreateEntity();

            entityManager.AddComponentData(
                requestEntity,
                new UnitSpawnRequestComponent()
                {
                    UnitCode = unitCode,
                    Position = spawnPosition
                }
            );
        }
    }
}

