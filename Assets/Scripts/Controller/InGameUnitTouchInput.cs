using System;
using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class InGameUnitTouchInput : InGameTouchInputBase, IActionMapInputer
    {
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;
        
        [SerializeField] private ActionMaps actionMap;
        
        private bool isInputActive;

        public bool IsActivated => isInputActive;
        
        
        
        public void ActionMapActivated()
        {
            isInputActive = true;
            OnActionMapInputerActivated?.Invoke(this);
        }
        public void ActionMapDeactivated()
        {
            isInputActive = false;
            OnActionMapInputerDeactivated?.Invoke(this);
        }
        public ActionMaps GetActionMap() => actionMap;
    }
}
