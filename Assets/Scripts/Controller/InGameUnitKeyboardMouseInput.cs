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

        private InputStatus inputStatus;
        private MoveScreenJudger moveScreenJudger;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;

        public bool IsActivated => isInputActive;

        protected override void Update()
        {
            if (!isInputActive) return;

            base.Update();
        }

        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger,
            InputStatus inputStatus)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;
            this.inputStatus = inputStatus;
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
                    if (inputStatus.IsShiftPressed)
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

                if (inputStatus.IsShiftPressed)
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

            if (inputStatus.IsShiftPressed)
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
                inputStatus.SetShift(true);
            }

            if (context.canceled)
            {
                inputStatus.SetShift(false);
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

