using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Assets.Scripts.Controller
{
    public class CreateMapTouchInput : TouchInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<TouchInputBase> OnActionMapInputerActivated;
        public event Action<TouchInputBase> OnActionMapInputerDeactivated;

        private SelectableController selectableController;
        private MoveScreenJudger moveScreenJudger;
        private NodeList nodeList;
        private Camera mainCamera;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;
        private bool isScreenMoving;

        public bool IsInputActive => isInputActive;

        public void Initialize(SelectableController selectableController, MoveScreenJudger moveScreenJudger,
                    NodeList nodeList)
        {
            this.selectableController = selectableController;
            this.moveScreenJudger = moveScreenJudger;
            this.nodeList = nodeList;

            mainCamera = Camera.main;
        }

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            Touch0Active = context.ReadValueAsButton();

            if (context.started)
            {
                CheckTouch0IsOverGameObject();

                JudgeScreenMoveOrDrag();
            }

            if (context.canceled)
            {
                isScreenMoving = false;
                OnMoveScreen?.Invoke(Vector2.zero);

                if (isPointerOverGameObject)
                {
                    return;
                }

                // node 선택 판정
                Vector2 origin = Camera.main.ScreenToWorldPoint(
                    new Vector3(Touch0Pos.x, Touch0Pos.y, -Camera.main.transform.position.z)
                );

                var index = nodeList.GetNodeIndex(origin);
                if (nodeList.TryGetNode(index, out Node node))
                {
                    selectableController.Selected(node);
                }
                else
                {
                    selectableController.Selected(null);
                }
            }
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

        public void OnTouch1Contact(InputAction.CallbackContext context)
        {
            Touch1Active = context.ReadValueAsButton();
        }

        public virtual void OnTouch1Position(InputAction.CallbackContext context)
        {
            Touch1Pos = context.ReadValue<Vector2>();
        }

        public void JudgeScreenMoveOrDrag()
        {
            if (!Touch0Active || Touch1Active) return;
            if (isPointerOverGameObject) return;

            Vector3 position = Touch0Pos;

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
