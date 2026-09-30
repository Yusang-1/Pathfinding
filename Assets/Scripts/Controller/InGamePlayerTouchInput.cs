using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Assets.Scripts.ControllUnit;
using System.Collections.Generic;

namespace Assets.Scripts.Controller
{
    public class InGamePlayerTouchInput : InGameTouchInputBase, IActionMapInputer, IHavePlayerTouchBuffer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<InGameTouchInputBase> OnActionMapInputerActivated;
        public event Action<InGameTouchInputBase> OnActionMapInputerDeactivated;

        private MoveScreenJudger moveScreenJudger;

        private Camera mainCamera;
        private readonly InputOnUIJudgeBuffer<IHavePlayerTouchBuffer.CommandType> inputOnUIJudgeBuffer = new();

        [SerializeField] private ActionMaps actionMap;

        private readonly Dictionary<IHavePlayerTouchBuffer.CommandType,
            Action<BufferedCommand<IHavePlayerTouchBuffer.CommandType>>> commandActionDict = new();

        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsActivated => isInputActive;

        private void LateUpdate()
        {
            inputOnUIJudgeBuffer.LateUpdate();
        }
        
        public void Initialize(UnitSelector unitSelector, MoveScreenJudger moveScreenJudger)
        {
            base.Initialize(unitSelector);
            this.moveScreenJudger = moveScreenJudger;

            mainCamera = Camera.main;

            InitializeCommandTypeActionDict();
        }

        public void InitializeCommandTypeActionDict()
        {
            commandActionDict.Add(IHavePlayerTouchBuffer.CommandType.CheckTouch0UI,
                CheckTouch0IsOverGameObject);
            commandActionDict.Add(IHavePlayerTouchBuffer.CommandType.JudgeScreenMoveOrDrag,
                JudgeScreenMoveOrDrag);
            commandActionDict.Add(IHavePlayerTouchBuffer.CommandType.Select,
                SelectUnit);
        }

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            Touch0Active = context.ReadValueAsButton();

            if (context.started)
            {
                // IsPointerOverGameObject를 buffer에 담아 LateUpdate에 실행
                var command = new BufferedCommand<IHavePlayerTouchBuffer.CommandType>
                (
                    this, IHavePlayerTouchBuffer.CommandType.CheckTouch0UI,
                    touchscreen.touches[0].touchId.ReadValue(), Touch0Pos, false
                );
                inputOnUIJudgeBuffer.Enqueue(command);

                command = new BufferedCommand<IHavePlayerTouchBuffer.CommandType>
                (
                    this, IHavePlayerTouchBuffer.CommandType.JudgeScreenMoveOrDrag,
                    0, Touch0Pos, false
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
                    var command = new BufferedCommand<IHavePlayerTouchBuffer.CommandType>
                    (
                        this, IHavePlayerTouchBuffer.CommandType.Select,
                        0, Touch0Pos, false
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
            }
        }

        public void ExecuteBufferedCommand(BufferedCommand<IHavePlayerTouchBuffer.CommandType> bufferedCommand)
        {
            commandActionDict[bufferedCommand.Type].Invoke(bufferedCommand);
        }

        public void CheckTouch0IsOverGameObject(BufferedCommand<IHavePlayerTouchBuffer.CommandType> bufferedCommand)
        {
            int touchId = bufferedCommand.TouchId;
            isPointerOverGameObject = eventSystem.IsPointerOverGameObject(touchId);
        }

        public void JudgeScreenMoveOrDrag(BufferedCommand<IHavePlayerTouchBuffer.CommandType> bufferedCommand)
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

        public void SelectUnit(BufferedCommand<IHavePlayerTouchBuffer.CommandType> bufferedCommand)
        {
            if (isPointerOverGameObject) return;
            
            var position = bufferedCommand.Position;

            var worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(position.x, position.y, -mainCamera.transform.position.z)
            );
            unitSelector.CheckPointFocused(worldPos);
            unitSelector.SelectFocused();
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
