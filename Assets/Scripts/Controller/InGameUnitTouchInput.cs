using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;
using System.Collections.Generic;

namespace Assets.Scripts.Controller
{
    public class InGameUnitTouchInput : InGameTouchInputBase, IActionMapInputer, IHaveUnitTouchBuffer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;

        private readonly InputOnUIJudgeBuffer<IHaveUnitTouchBuffer.CommandType> inputOnUIJudgeBuffer = new();
        private InputStatus inputStatus;
        private MoveScreenJudger moveScreenJudger;
        private Camera mainCamera;

        [SerializeField] private ActionMaps actionMap;

        private readonly Dictionary<IHaveUnitTouchBuffer.CommandType,
            Action<BufferedCommand<IHaveUnitTouchBuffer.CommandType>>> commandActionDict = new();

        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsActivated => isInputActive;

        private void LateUpdate()
        {
            inputOnUIJudgeBuffer.LateUpdate();
        }

        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger,
            InputStatus inputStatus)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;
            this.inputStatus = inputStatus;

            mainCamera = Camera.main;

            InitializeCommandTypeActionDict();
        }

        public void InitializeCommandTypeActionDict()
        {
            commandActionDict.Add(IHaveUnitTouchBuffer.CommandType.CheckTouch0UI,
                CheckTouch0IsOverGameObject);
            commandActionDict.Add(IHaveUnitTouchBuffer.CommandType.JudgeScreenMoveOrDrag,
                JudgeScreenMoveOrDrag);
            commandActionDict.Add(IHaveUnitTouchBuffer.CommandType.SelectOrMove,
                SelectOrMove);
        }

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            Touch0Active = context.ReadValueAsButton();

            if (context.started)
            {
                // IsPointerOverGameObject를 buffer에 담아 LateUpdate에 실행
                var command = new BufferedCommand<IHaveUnitTouchBuffer.CommandType>
                (
                    this, IHaveUnitTouchBuffer.CommandType.CheckTouch0UI,
                    touchscreen.touches[0].touchId.ReadValue(), Touch0Pos, inputStatus.IsShiftPressed
                );
                inputOnUIJudgeBuffer.Enqueue(command);

                command = new BufferedCommand<IHaveUnitTouchBuffer.CommandType>
                (
                    this, IHaveUnitTouchBuffer.CommandType.JudgeScreenMoveOrDrag,
                    0, Touch0Pos, inputStatus.IsShiftPressed
                );
                inputOnUIJudgeBuffer.Enqueue(command);
            }

            if (context.canceled)
            {
                isScreenMoving = false;
                OnMoveScreen?.Invoke(Vector2.zero);

                if (JudgeHoldCoroutine != null)
                {
                    StopCoroutine(JudgeHoldCoroutine);
                }

                if (IsHold)
                {
                    HoldCanceled();
                }
                else
                {
                    var command = new BufferedCommand<IHaveUnitTouchBuffer.CommandType>
                    (
                        this, IHaveUnitTouchBuffer.CommandType.SelectOrMove,
                        0, Touch0Pos, inputStatus.IsShiftPressed
                    );
                    inputOnUIJudgeBuffer.Enqueue(command);
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

                JudgeHoldByDelta();
            }
        }

        protected override void HoldCanceled()
        {
            IsHold = false;
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

        public void ExecuteBufferedCommand(BufferedCommand<IHaveUnitTouchBuffer.CommandType> bufferedCommand)
        {
            commandActionDict[bufferedCommand.Type].Invoke(bufferedCommand);
        }

        public void CheckTouch0IsOverGameObject(BufferedCommand<IHaveUnitTouchBuffer.CommandType> bufferedCommand)
        {
            int touchId = bufferedCommand.TouchId;
            isPointerOverGameObject = eventSystem.IsPointerOverGameObject(touchId);
        }

        public void SelectOrMove(BufferedCommand<IHaveUnitTouchBuffer.CommandType> bufferedCommand)
        {
            if (isPointerOverGameObject) return;

            Vector3 position = bufferedCommand.Position;
            bool isShiftPressed = bufferedCommand.ShiftPressed;

            var worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(position.x, position.y, -mainCamera.transform.position.z)
            );

            // 선택한 곳이 유닛이면 선택, 땅이면 이동
            bool isSelect = unitSelector.TryCheckPointFocused(worldPos);
            if (isSelect)
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
            else
            {
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

        public void JudgeScreenMoveOrDrag(BufferedCommand<IHaveUnitTouchBuffer.CommandType> bufferedCommand)
        {
            if (!Touch0Active) return;
            if (isPointerOverGameObject) return;

            Vector3 position = bufferedCommand.Position;

            var viewPortPosition = mainCamera.ScreenToViewportPoint(position);
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
                JudgeHoldCoroutine = WaitDrag(position);
                StartCoroutine(JudgeHoldCoroutine);
            }
        }

        private void JudgeHoldByDelta()
        {
            if (isJudgingHold && Touch0Delta.sqrMagnitude > 0.02f)
            {
                if (JudgeHoldCoroutine != null)
                {
                    StopCoroutine(JudgeHoldCoroutine);
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
