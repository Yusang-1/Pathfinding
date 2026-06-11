using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectable, IHaveOwnActionMap
    {
        public event Action<ISelectable> OnSelectedCallback;
        public event Action<ISelectable> OnDeselectedCallback;
        public event Action<string> OnEnableActionMap;
        public event Action OnDisableActionMap;

        [SerializeField] private UnitController controller;

        private void Update()
        {
            controller.ControllerUpdate();
        }

        public void Selected()
        {
            OnSelectedCallback?.Invoke(this);
            OnEnableActionMap?.Invoke(nameof(Unit));
        }

        public void Deselected()
        {
            OnDeselectedCallback?.Invoke(this);
            OnDisableActionMap?.Invoke();
        }
    }
}

public interface IHaveOwnActionMap
{
    public event Action<string> OnEnableActionMap;
    public event Action OnDisableActionMap;
}
