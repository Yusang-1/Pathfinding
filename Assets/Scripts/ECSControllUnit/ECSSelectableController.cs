using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSSelectableController
    {
        public void MakeSelectionRequest(Vector3 position, bool isAdditive)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity selectionRequest = entityManager.CreateEntity();
            
            entityManager.AddComponentData(
                selectionRequest,
                new UnitSelectionRequest()
                {
                    WorldPosition = position, IsAdditive = isAdditive
                }
            );
        }
        
        public void MakeAreaSelectionRequest()
        {
            
        }
    }
}
