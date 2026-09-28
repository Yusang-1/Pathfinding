using System;
using Assets.Scripts.ECSControllUnit;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private Vector3 touch0WorldPos;
        private bool isInputActive;

        public bool IsActivated => isInputActive;

        public void Initialize(ECSSelectableController selectableController, MoveScreenJudger moveScreenJudger)
        {
            this.selectableController = selectableController;
            this.moveScreenJudger = moveScreenJudger;
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
                WaitDragCoroutine = WaitDrag(Touch0Pos);
                StartCoroutine(WaitDragCoroutine);
            }

            if (context.performed)
            {
                if (Touch0Delta.sqrMagnitude > 0.02f)
                {
                    StopCoroutine(WaitDragCoroutine);
                }
            }

            if (context.canceled)
            {
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
                    touch0WorldPos = Camera.main.ScreenToWorldPoint(
                        new Vector3(Touch0Pos.x, Touch0Pos.y, -Camera.main.transform.position.z)
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
