using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Controller
{
    public class TouchInput : MonoBehaviour
    {
        public bool Touch0Active { get; private set; }
        public Vector2 Touch0Delta { get; private set; }
        public Vector2 Touch0Pos { get; private set; }
        public bool Touch1Active { get; private set; }
        public Vector2 Touch1Pos { get; private set; }

        public void OnTouch0Contact(InputAction.CallbackContext context)
        {
            // 버튼 형태는 ReadValueAsButton()이나 IsPressed()로 상태 확인 가능
            Touch0Active = context.ReadValueAsButton();
        }

        public void OnTouch0Delta(InputAction.CallbackContext context)
        {
            Touch0Delta = context.ReadValue<Vector2>();
        }

        public void OnTouch0Position(InputAction.CallbackContext context)
        {
            Touch0Pos = context.ReadValue<Vector2>();
        }

        public void OnTouch1Contact(InputAction.CallbackContext context)
        {
            Touch1Active = context.ReadValueAsButton();
        }

        public void OnTouch1Position(InputAction.CallbackContext context)
        {
            Touch1Pos = context.ReadValue<Vector2>();
        }                
    }
}
