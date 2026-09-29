using UnityEngine;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller.UI
{
    public class UIForTouchMeditator : MonoBehaviour
    {
        public event Action OnShiftPressed;
        [SerializeField] private UIShiftButton uiShiftButton;

        private void OnEnable()
        {
            uiShiftButton.OnShiftPressed += HandlerShiftPressed;
        }

        private void OnDisable()
        {
            uiShiftButton.OnShiftPressed -= HandlerShiftPressed;
        }

        public void ActiveIfControllSchemeTouch(ControllScheme controllScheme)
        {
            if (controllScheme == ControllScheme.Touch)
            {
                SetActiveTrue();
            }
            else
            {
                SetActiveFalse();
            }
        }

        private void HandlerShiftPressed()
        {
            OnShiftPressed?.Invoke();
        }

        public void SetActiveTrue() => gameObject.SetActive(true);
        public void SetActiveFalse() => gameObject.SetActive(false);
    }
}
