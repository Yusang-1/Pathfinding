using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class Unit : MonoBehaviour, ISelectable
    {
        public event Action<ISelectable> OnSelectedCallback;
        public event Action<ISelectable> OnDeselectedCallback;

        [SerializeField] private UnitController controller;

        private void Update()
        {
            controller.ControllerUpdate();
        }

        public void Selected()
        {
            OnSelectedCallback?.Invoke(this);
        }

        public void Deselected()
        {
            OnDeselectedCallback?.Invoke(this);
        }
    }
}
