using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ECSControllUnit;

namespace Assets.Scripts.Controller
{
    public class InGamePlayerTouchInputECS : InGameTouchInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;
        public new event Func<Vector3, Vector3?> OnHoldPerformed;

        private MoveScreenJudger moveScreenJudger;
        private ECSSelectableController selectableController;

        [SerializeField] private ActionMaps actionMap;
        private Camera mainCamera;

        private Vector3 touch0WorldPos;
        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsActivated => isInputActive;

        public void Initialize(ECSSelectableController selectableController, MoveScreenJudger moveScreenJudger)
        {
            this.selectableController = selectableController;
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
                    StopCoroutine(WaitDragCoroutine);
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
                    touch0WorldPos = mainCamera.ScreenToWorldPoint(
                        new Vector3(Touch0Pos.x, Touch0Pos.y, -mainCamera.transform.position.z)
                    );
                    selectableController.MakeSelectionRequest(touch0WorldPos, false);
                }
            }
        }

        private Vector3? holdPerformedWorldPosition;
        protected override void HoldPerformed()
        {
            holdPerformedWorldPosition = OnHoldPerformed?.Invoke(Touch0Pos);
            if (holdPerformedWorldPosition == null) return;

            Vector3 position = (Vector3)holdPerformedWorldPosition;
            selectableController.CheckUnitsInArea(touch0WorldPos, position);
        }

        protected override void HoldCanceled()
        {
            IsDrag = false;
            if (holdPerformedWorldPosition == null) return;

            selectableController.MakeSelectionRequest(touch0WorldPos, false);
            InvokeOnHoldCanceled();
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
