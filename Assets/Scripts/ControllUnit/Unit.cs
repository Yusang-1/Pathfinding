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

        [SerializeField] private UnitController controller;
        [SerializeField] private UnitBottomSelectChanger bottomChanger;
        [SerializeField] private UnitSO unitData;

        private UnitInput unitInput;
        
        private UnitBottomStatus bottomStatus;
        public Vector2Int CurrentKey;        

        private void Update()
        {
            controller.ControllerUpdate();
        }

        public void Initialize(SpatialHash spatialHash)
        {
            controller.Initialize(this, spatialHash);
        }

        public void UnitSpawned()
        {
            OnPoolObjectFirstCreated?.Invoke(this);
            gameObject.SetActive(true);
        }

        public void UnitDespawned()
        {
            OnPoolObjectUnused?.Invoke(this);
            gameObject.SetActive(false);
            
            bottomStatus = UnitBottomStatus.None;
            bottomChanger.StatusChanged(bottomStatus);
        }

        public void Selected()
        {
            if (unitInput == null)
            {
                unitInput = FindAnyObjectByType<UnitInput>();
            }
            unitInput.OnRightClickRequested += MoveUnit;
            
            bottomStatus = UnitBottomStatus.Selected;
            bottomChanger.StatusChanged(bottomStatus);
            OnSelectedCallback?.Invoke(this);
        }

        public void Deselected()
        {
            unitInput.OnRightClickRequested -= MoveUnit;

            bottomStatus = UnitBottomStatus.None;
            bottomChanger.StatusChanged(bottomStatus);
            OnDeselectedCallback?.Invoke(this);
        }

        public void Focused()
        {
            if(bottomStatus == UnitBottomStatus.Selected) return;
            
            bottomStatus = UnitBottomStatus.Focused;
            bottomChanger.StatusChanged(bottomStatus);
        }

        public void Unfocused()
        {
            if(bottomStatus == UnitBottomStatus.Selected) return;
            
            bottomChanger.StatusChanged(UnitBottomStatus.None);
        }

        public SelectableType GetSelectableType() => unitData.SelectableType;

        private void MoveUnit(Vector3 destination)
        {
            controller.MoveTo(destination);
        }

        public string GetActionMapName() => unitData.ActionMapName;
    }
}

public interface IHaveOwnActionMap
{
    public string GetActionMapName();
}
