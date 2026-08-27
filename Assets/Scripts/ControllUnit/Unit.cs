using UnityEngine;
using System;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectableUnit, IHaveOwnActionMap, IPoolObject<Unit>
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        public event Action<ISelectableUnit> OnDespawnedCallback;
        public event Action<Unit> OnPoolObjectFirstCreated;
        public event Action<Unit> OnPoolObjectUnused;

        [SerializeField] private UnitSO unitData;
        [SerializeField] private SteeringWeightingSO steeringWeightingData;

        private UnitController controller;
        private UnitBottomSelectChanger bottomChanger;

        private UnitBottomStatus bottomStatus;
        public Vector2Int CurrentKey;

        public UnitSO UnitData => unitData;
        public UnitController Controller => controller;
        
        public bool IsEventBound;
        
        private void Update()
        {
            controller.ControllerUpdate();
        }

        private void LateUpdate()
        {
            controller.ControllerLateUpdate();
        }

        public void Initialize(UnitRuntimeContext unitRuntimeContext, UnitBottomSelectChanger bottomChanger)
        {
            controller = new UnitController(this, unitRuntimeContext, bottomChanger.transform, steeringWeightingData.WalkConfig, unitData); this.bottomChanger = bottomChanger;
            bottomChanger.SetRadius(unitData.Radius);
        }

        public void UnitSpawned()
        {
            OnPoolObjectFirstCreated?.Invoke(this);
            bottomChanger.Initialize();
            gameObject.SetActive(true);
        }

        public void UnitDespawned()
        {
            OnDespawnedCallback?.Invoke(this);
            OnPoolObjectUnused?.Invoke(this);
            gameObject.SetActive(false);
            bottomChanger.Despawned();
            ChangeBottomStatus(UnitBottomStatus.None);
        }

        public void Selected()
        {
            ChangeBottomStatus(UnitBottomStatus.Selected);
            OnSelectedCallback?.Invoke(this);
        }

        public void Deselected()
        {
            ChangeBottomStatus(UnitBottomStatus.None);
            OnDeselectedCallback?.Invoke(this);
        }

        public void Focused()
        {
            if (bottomStatus == UnitBottomStatus.Selected) return;

            ChangeBottomStatus(UnitBottomStatus.Focused);
        }

        public void Unfocused()
        {
            if (bottomStatus == UnitBottomStatus.Selected) return;

            ChangeBottomStatus(UnitBottomStatus.None);
        }

        public SelectableType GetSelectableType() => unitData.SelectableType;

        private void ChangeBottomStatus(UnitBottomStatus status)
        {
            bottomStatus = status;
            bottomChanger.StatusChanged(bottomStatus);
        }

        public ActionMaps GetActionMapName() => unitData.ActionMap;
    }
}

public interface IHaveOwnActionMap
{
    public ActionMaps GetActionMapName();
}
