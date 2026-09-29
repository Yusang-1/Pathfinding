using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class InputStatus
    {
        public bool IsShiftPressed {get; private set;}
        
        public void ToggleShift()
        {
            IsShiftPressed = !IsShiftPressed;
        }
        public void SetShift(bool value)
        {
            IsShiftPressed = value;
        }
    }
}
