using UnityEngine;
using Assets.Scripts.CrowdSimulation.UI;
using Unity.Entities;

namespace Assets.Scripts.ECS
{
    public class ECSUnitSpawner : MonoBehaviour
    {
        [SerializeField] private CrowdUnitAuthoring unitPrefab;
        [SerializeField] private UISpawnUnit uiSpawnUnit;

        private void Start()
        {
            uiSpawnUnit.OnSpawnUnitRequested += SpawnUnit;                        
        }

        public void SpawnUnit()
        {
            // 1. 기본 ECS 월드의 EntityManager 가져오기
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 2. 빈 엔티티 생성 후 요청 컴포넌트 추가 (신호 발송)
            Entity requestEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(requestEntity, new SpawnRequestData());
        }
    }
}

