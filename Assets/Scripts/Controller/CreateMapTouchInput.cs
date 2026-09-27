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

        public override void OnTouch0Contact(InputAction.CallbackContext context)
        {
            if (isPointerOverGameObject)
            {
                if (!(context.canceled && IsDrag)) return;
            }

            base.OnTouch0Contact(context);

            if (context.canceled)
            {
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
                
                // screen이동 판정
                var viewPortPosition = Camera.main.ScreenToViewportPoint(Touch0Pos);
                if(moveScreenJudger.TryGetScreenMoveVelocity(viewPortPosition, out Vector2 velocity))
                {
                    OnMoveScreen?.Invoke(velocity);
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
