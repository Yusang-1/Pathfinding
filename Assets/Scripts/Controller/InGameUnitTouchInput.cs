using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGameUnitTouchInput : InGameTouchInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;
        
        private MoveScreenJudger moveScreenJudger;
        
        [SerializeField] private ActionMaps actionMap;
        
        private bool isInputActive;

        public bool IsActivated => isInputActive;
        
        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;
        }
        
        public override void OnTouch0Position(InputAction.CallbackContext context)
        {
            base.OnTouch0Position(context);

            // screen이동 판정
            var viewPortPosition = Camera.main.ScreenToViewportPoint(Touch0Pos);
            if (moveScreenJudger.TryGetScreenMoveVelocity(viewPortPosition, out Vector2 velocity))
            {
                OnMoveScreen?.Invoke(velocity);
            }
        }
        
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
