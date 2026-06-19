using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectableUnit, IHaveOwnActionMap, IPoolObject<Unit>
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        public event Action<Unit> OnPoolObjectFirstCreated;
        public event Action<Unit> OnPoolObjectUnused;

        [SerializeField] private UnitSO unitData;

        private UnitController controller;        
        private UnitInput unitInput;
        private UnitBottomSelectChanger bottomChanger;        
        
        private UnitBottomStatus bottomStatus;
        public Vector2Int CurrentKey;        

        private void Update()
        {
            controller.ControllerUpdate();
        }

        private void LateUpdate()
        {
            controller.ControllerLateUpdate();
        }

        public void Initialize(SpatialHash spatialHash, UnitBottomSelectChanger bottomChanger)
        {
            controller = new UnitController(this, spatialHash, bottomChanger.transform, unitData, FindAnyObjectByType<Pathfinder>());
            this.bottomChanger = bottomChanger;
        }

        public void UnitSpawned()
        {
            OnPoolObjectFirstCreated?.Invoke(this);
            bottomChanger.Initialize();
            gameObject.SetActive(true);
        }

        public void UnitDespawned()
        {
            OnPoolObjectUnused?.Invoke(this);
            gameObject.SetActive(false);
            bottomChanger.Despawned();
            ChangeBottomStatus(UnitBottomStatus.None);
        }

        public void Selected()
        {
            if (unitInput == null)
            {
                unitInput = FindAnyObjectByType<UnitInput>();
            }
            unitInput.OnRightClickRequested += MoveUnit;
            
            ChangeBottomStatus(UnitBottomStatus.Selected);
            OnSelectedCallback?.Invoke(this);
        }

        public void Deselected()
        {
            unitInput.OnRightClickRequested -= MoveUnit;

            ChangeBottomStatus(UnitBottomStatus.None);
            OnDeselectedCallback?.Invoke(this);
        }

        public void Focused()
        {
            if(bottomStatus == UnitBottomStatus.Selected) return;
            
            ChangeBottomStatus(UnitBottomStatus.Focused);
        }

        public void Unfocused()
        {
            if(bottomStatus == UnitBottomStatus.Selected) return;
            
            ChangeBottomStatus(UnitBottomStatus.None);
        }

        public SelectableType GetSelectableType() => unitData.SelectableType;

        private void MoveUnit(Vector3 destination)
        {
            controller.MoveTo(destination);
        }
        
        private void ChangeBottomStatus(UnitBottomStatus status)
        {
            bottomStatus = status;
            bottomChanger.StatusChanged(bottomStatus);
        }

        public string GetActionMapName() => unitData.ActionMapName;
    }
}

public interface IHaveOwnActionMap
{
    public string GetActionMapName();
}
