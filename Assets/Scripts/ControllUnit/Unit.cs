using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectableUnit, IHaveOwnActionMap
    {
        public event Action<ISelectableUnit> OnSelectedCallback;
        public event Action<ISelectableUnit> OnDeselectedCallback;
        public event Action<string> OnEnableActionMap;
        public event Action OnDisableActionMap;

        [SerializeField] private UnitController controller;
        private UnitInput unitInput;

        private void Update()
        {
            controller.ControllerUpdate();
        }

        public void Selected()
        {
            if(unitInput == null)
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
        
        public void MoveUnit(Vector3 destination)
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
