using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectableUnit, IHaveOwnActionMap, IPoolObject<Unit>
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        public event Action<string> OnEnableActionMap;
        public event Action OnDisableActionMap;
        public event Action<Unit> OnPoolObjectFirstCreated;
        public event Action<Unit> OnPoolObjectUnused;

        [SerializeField] private UnitController controller;
        private UnitInput unitInput;

        private void Update()
        {
            controller.ControllerUpdate();
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
        }

        public void Selected()
        {
            if (unitInput == null)
            {
                unitInput = FindAnyObjectByType<UnitInput>();
            }
            unitInput.OnRightClickRequested += MoveUnit;

            OnSelectedCallback?.Invoke(this);
            OnEnableActionMap?.Invoke(nameof(Unit));
        }

        public void Deselected()
        {
            unitInput.OnRightClickRequested -= MoveUnit;

            OnDeselectedCallback?.Invoke(this);
            OnDisableActionMap?.Invoke();
        }

        private void MoveUnit(Vector3 destination)
        {
            controller.MoveTo(destination);
        }
    }
}

public interface IHaveOwnActionMap
{
    public event Action<string> OnEnableActionMap;
    public event Action OnDisableActionMap;
}
