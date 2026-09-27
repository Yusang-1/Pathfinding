using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGameUnitKeyboardMouseInput : InGameKeyboardMouseInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerActivated;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerDeactivated;

        private MoveScreenJudger moveScreenJudger;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;
        private bool isShiftPressed;

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

        public override void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive)
            {
                if (!(context.canceled && isDrag)) return;
            }

            if (context.started)
            {
                holdJudgementCoroutine = HoldJudgement(mousePosition);
                StartCoroutine(holdJudgementCoroutine);
            }

            if (context.canceled)
            {
                StopCoroutine(holdJudgementCoroutine);

                if (isDrag)
                {
                    HoldCanceled();
                }
                else
                {
                    if (isShiftPressed)
                    {
                        unitSelector.ShiftSelectedFocused();
                    }
                    else
                    {
                        unitSelector.SelectFocused();
                    }
                }
            }
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive) return;

            if (context.canceled)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

                if (isShiftPressed)
                {
                    unitSelector.ShiftRightClickMove(worldPos);
                }
                else
                {
                    unitSelector.RightClickMove(worldPos);
                }
            }
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

        protected override void HoldCanceled()
        {
            isDrag = false;

            if (isShiftPressed)
            {
                unitSelector.ShiftSelectedFocusedList();
            }
            else
            {
                unitSelector.SelectFocused();
            }
            InvokeOnHoldCanceled();
        }

        public void OnPressShift(InputAction.CallbackContext context)
        {
            if (!isInputActive) return;

            if (context.started)
            {
                isShiftPressed = true;

            }

            if (context.canceled)
            {
                isShiftPressed = false;

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

