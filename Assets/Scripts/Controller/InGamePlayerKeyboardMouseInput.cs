using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGamePlayerKeyboardMouseInput : InGameKeyboardMouseInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerActivated;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerDeactivated;

        private MoveScreenJudger moveScreenJudger;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;

        public bool IsActivated => isInputActive;

        protected override void Update()
        {
            if (!isInputActive) return;

            base.Update();
        }

        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;
        }

        public override void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            base.OnTrackMousePosition(context);

            Vector3 viewportPos = mainCamera.ScreenToViewportPoint(mousePosition);
            if (moveScreenJudger.TryGetScreenMoveVelocity(viewportPos, out Vector2 velocity))
            {
                OnMoveScreen?.Invoke(velocity);
            }
            else
            {
                OnMoveScreen?.Invoke(Vector2.zero);
            }

            if (isDrag || isPointerOverGameObject) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
            unitSelector.CheckPointFocused(worldPos);
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
