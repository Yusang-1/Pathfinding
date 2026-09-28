using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGamePlayerTouchInput : InGameTouchInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;

        private MoveScreenJudger moveScreenJudger;

        [SerializeField] private ActionMaps actionMap;
        private Camera mainCamera;

        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsActivated => isInputActive;

        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;

            mainCamera = Camera.main;
        }

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject)
            {
                if (!(context.canceled && IsDrag)) return;
            }

            Touch0Active = context.ReadValueAsButton();

            if (context.started)
            {
                var viewPortPosition = mainCamera.ScreenToViewportPoint(Touch0Pos);
                if (moveScreenJudger.TryGetScreenMoveVelocity(viewPortPosition, out Vector2 velocity))
                {
                    isScreenMoving = true;
                }
                else
                {
                    isScreenMoving = false;
                }

                if (!isScreenMoving)
                {
                    WaitDragCoroutine = WaitDrag(Touch0Pos);
                    StartCoroutine(WaitDragCoroutine);
                }
            }

            if (context.performed)
            {
                if (isScreenMoving) return;

                if (Touch0Delta.sqrMagnitude > 0.02f)
                {
                    if (WaitDragCoroutine != null)
                    {
                        StopCoroutine(WaitDragCoroutine);
                    }
                }
            }

            if (context.canceled)
            {
                isScreenMoving = false;
                OnMoveScreen?.Invoke(Vector2.zero);

                if (WaitDragCoroutine != null)
                {
                    StopCoroutine(WaitDragCoroutine);
                }

                if (IsDrag)
                {
                    HoldCanceled();
                }
                else
                {
                    var worldPos = mainCamera.ScreenToWorldPoint(
                        new Vector3(Touch0Pos.x, Touch0Pos.y, -mainCamera.transform.position.z)
                    );
                    unitSelector.CheckPointFocused(worldPos);
                    unitSelector.SelectFocused();
                }
            }
        }

        public override void OnTouch0Position(InputAction.CallbackContext context)
        {
            base.OnTouch0Position(context);

            if (context.performed)
            {
                // screen이동 판정
                if (isScreenMoving)
                {
                    var viewPortPosition = mainCamera.ScreenToViewportPoint(Touch0Pos);
                    if (moveScreenJudger.TryGetScreenMoveVelocity(viewPortPosition, out Vector2 velocity))
                    {
                        OnMoveScreen?.Invoke(velocity);
                    }
                    else
                    {
                        OnMoveScreen?.Invoke(Vector2.zero);
                    }
                }
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
