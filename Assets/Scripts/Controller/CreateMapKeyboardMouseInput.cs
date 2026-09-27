using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Controller
{
    public class CreateMapKeyboardMouseInput : KeyboardMouseInputBase, IActionMapInputer
    {
        public event Action<Vector2> OnMoveScreen;
        public event Action<KeyboardMouseInputBase> OnActionMapInputerActivated;
        public event Action<KeyboardMouseInputBase> OnActionMapInputerDeactivated;

        private SelectableController selectableController;
        private MoveScreenJudger moveScreenJudger;
        private NodeList nodeList;

        [SerializeField] private ActionMaps actionMap;

        private bool isInputActive;

        public bool IsInputActive => isInputActive;

        public void Initialize(SelectableController selectableController, MoveScreenJudger moveScreenJudger,
            NodeList nodeList)
        {
            this.selectableController = selectableController;
            this.moveScreenJudger = moveScreenJudger;
            this.nodeList = nodeList;
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
        }

        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject) return;

            if (context.canceled)
            {
                Vector2 origin = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

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

        public void OnPressESC(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                InvokeOnControllMenu();
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
