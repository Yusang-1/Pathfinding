using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using System;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSSelectableController
    {
        public event Action<string, Entity> OnSelectedCallback;
        public event Action<Entity> OnDeselectedCallback;

        private Action<ActionMaps> OnchangeActionMapSelected;
        private Action OnchangeActionMapDefault;

        public event Action<Vector3> OnMove;
        public event Action<Vector3> OnMoveAdditive;

        private EntityManager entityManager;
        private EntityQuery SelectedQuery;
        private EntityQuery DeselectedQuery;
        
        public void Initialize()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            SelectedQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<UnitSelectedData>());
            DeselectedQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<UnitDeselectedData>());
        }

        public void MakeSelectionRequest(Vector3 position, bool isAdditive)
        {
            Entity selectionRequest = entityManager.CreateEntity();

            entityManager.AddComponentData(
                selectionRequest,
                new UnitSelectionRequest()
                {
                    WorldPosition = position,
                    IsAdditive = isAdditive
                }
            );
        }

        public void MakeAreaSelectionRequest()
        {

        }

        public void MakeMoveCommand(Vector3 destination, bool isAdditive)
        {
            if (isAdditive)
            {
                OnMoveAdditive?.Invoke(destination);
            }
            else
            {
                OnMove?.Invoke(destination);
            }
        }

        public void SelectedUpdate()
        {
            ManageSelected();
            
            ManageDeselected();            
        }
        
        private void ManageSelected()
        {
            using NativeArray<UnitSelectedData> datas = SelectedQuery.ToComponentDataArray<UnitSelectedData>(Allocator.Temp);            
            
            if(datas == null || datas.Length == 0)
            {
                return;
            }
            
            using NativeArray<Entity> entities = SelectedQuery.ToEntityArray(Allocator.Temp);
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            int count = 0;
            foreach(UnitSelectedData data in datas)
            {
                string name = data.EntityName.ToString(); // 가비지 줄이기 위해 수정해야할듯
                OnSelectedCallback?.Invoke(name, data.Entity);
                
                ecb.DestroyEntity(entities[count]);
                count++;
            }
            
            ecb.Playback(entityManager);
        }
        
        private void ManageDeselected()
        {
            using NativeArray<UnitDeselectedData> datas = DeselectedQuery.ToComponentDataArray<UnitDeselectedData>(Allocator.Temp);            
            
            if(datas == null || datas.Length == 0)
            {
                return;
            }
            
            using NativeArray<Entity> entities = DeselectedQuery.ToEntityArray(Allocator.Temp);
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            int count = 0;
            foreach(UnitDeselectedData data in datas)
            {
                OnDeselectedCallback?.Invoke(data.Entity);
                
                ecb.DestroyEntity(entities[count]);
                count++;
            }
            
            ecb.Playback(entityManager);
        }

        public void UnitSelected(string name, Entity entity)
        {
            OnSelectedCallback?.Invoke(name, entity);
        }

        public void UnitDeselected(Entity entity)
        {
            OnDeselectedCallback?.Invoke(entity);
        }

        public void GetActions(Action<ActionMaps> changeActionMapSelected, Action changeActionMapDefault)
        {
            OnchangeActionMapSelected = changeActionMapSelected;
            OnchangeActionMapDefault = changeActionMapDefault;
        }
    }
}
