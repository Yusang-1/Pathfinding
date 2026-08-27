using UnityEngine;
using Unity.Entities;
using System;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSSelectableController
    {
        private Action<ActionMaps> OnchangeActionMapSelected;
        private Action OnchangeActionMapDefault;
        
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
        
        public void MakeMoveCommand(Vector3 destination, bool isAdditive)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity moveCommand = entityManager.CreateEntity();
            
            entityManager.AddComponentData(
                moveCommand,
                new UnitMoveCommandComponent()
                {
                    Destination = destination, IsAdditive = isAdditive
                }
            );
        }
        
        public void GetActions(Action<ActionMaps> changeActionMapSelected, Action changeActionMapDefault)
        {
            OnchangeActionMapSelected = changeActionMapSelected;
            OnchangeActionMapDefault = changeActionMapDefault;
        }
    }
}
