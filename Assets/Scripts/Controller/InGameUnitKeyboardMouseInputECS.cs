using UnityEngine;
using System;
using Assets.Scripts.ECSControllUnit;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Controller
{
    public class InGameUnitKeyboardMouseInputECS : InGameKeyboardMouseInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerActivated;
        public event Action<InGameKeyboardMouseInputBase> OnActionMapInputerDeactivated;
        public new event Func<Vector3, Vector3?> OnHoldPerformed;

        private InputStatus inputStatus;
        private MoveScreenJudger moveScreenJudger;
        private ECSSelectableController selectableController;

        [SerializeField] private ActionMaps actionMap;

        private Vector3 mouseWorldPosition;
        private bool isInputActive;

        public bool IsInputActive => isInputActive;

        protected override void Update()
        {
            if (!isInputActive) return;

            base.Update();
        }

        public void Initialize(ECSSelectableController selectableController, MoveScreenJudger moveScreenJudger,
            InputStatus inputStatus)
        {
            this.selectableController = selectableController;
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
                    selectableController.MakeSelectionRequest(mouseWorldPosition, inputStatus.IsShiftPressed);
                }
            }
        }

        private Vector3? holdPerformedWorldPosition;
        protected override void HoldPerformed()
        {
            holdPerformedWorldPosition = OnHoldPerformed?.Invoke(mousePosition);
            if (holdPerformedWorldPosition == null) return;

            Vector3 position = (Vector3)holdPerformedWorldPosition;
            selectableController.CheckUnitsInArea(mouseWorldPosition, position);
        }

        protected override void HoldCanceled()
        {
            isDrag = false;
            if (holdPerformedWorldPosition == null) return;

            selectableController.MakeSelectionRequest(mouseWorldPosition, false);
            InvokeOnHoldCanceled();
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject || !isInputActive) return;

            if (context.canceled)
            {
                selectableController.MakeMoveCommand(mouseWorldPosition, inputStatus.IsShiftPressed);
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

            if (context.performed)
            {
                if (isDrag || isPointerOverGameObject) return;

                mouseWorldPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z)
                );

                // 마우스 커서 아래 유닛이 있는지 확인
                selectableController.CheckUnitIsBelowMouse(mouseWorldPosition);
            }
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
