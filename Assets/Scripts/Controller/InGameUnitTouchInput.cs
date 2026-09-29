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

        private InputStatus inputStatus;
        private MoveScreenJudger moveScreenJudger;
        private Camera mainCamera;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsActivated => isInputActive;

        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger,
            InputStatus inputStatus)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;
            this.inputStatus = inputStatus;

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

                    // 선택한 곳이 유닛이면 선택, 땅이면 이동 
                    bool isSelect = unitSelector.TryCheckPointFocused(worldPos);
                    if (isSelect)
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
                    else
                    {
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
            }
        }

        public override void OnTouch0Position(InputAction.CallbackContext context)
        {
            base.OnTouch0Position(context);

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
        
        protected override void HoldCanceled()
        {
            IsDrag = false;
            if(inputStatus.IsShiftPressed)
            {
                unitSelector.ShiftSelectedFocusedList();
            }
            else
            {
                unitSelector.SelectFocused();
            }            
            InvokeOnHoldCanceled();
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
