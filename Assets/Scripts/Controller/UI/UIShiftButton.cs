using UnityEngine;
using UnityEngine.UI;
using System;

namespace Assets.Scripts.Controller.UI
{
    public class UIShiftButton : MonoBehaviour
    {
        public event Action OnShiftPressed;

        [SerializeField] private Image shiftImage;
        [SerializeField] private Color pressedColor;
        [SerializeField] private Color releasedColor;
        
        private bool isPressed;

        public void OnShift()
        {
            OnShiftPressed?.Invoke();
            
            isPressed = !isPressed;
            SetButtonColor();
        }
        
        private void SetButtonColor()
        {
            if(isPressed)
            {
                shiftImage.color = pressedColor;
            }
            else
            {
                shiftImage.color = releasedColor;
            }
        }
    }
}
