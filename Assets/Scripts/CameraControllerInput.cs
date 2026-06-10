using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class CameraControllerInput : MonoBehaviour
{
    public event Action<Vector2> OnDirectionChanged;
    private Vector2 sumOfDirection;
    private readonly Dictionary<int, Vector2> directionDict = new();
    
    public void OnMoveForward(InputAction.CallbackContext context)
    {
        HandleKeyInput('w', context);
    }

    public void OnMoveBackward(InputAction.CallbackContext context)
    {
        HandleKeyInput('s', context);
    }

    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        HandleKeyInput('a', context);
    }

    public void OnMoveRight(InputAction.CallbackContext context)
    {
        HandleKeyInput('d', context);
    }
        
    private void HandleKeyInput(int index, InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 value = context.ReadValue<Vector2>();
            directionDict[index] = value;
            SetDirection(value);
        }

        if (context.canceled)
        {
            Vector2 value = directionDict[index];
            SetDirection(-value);
        }
    }
    
    private void SetDirection(Vector2 dir)
    {
        sumOfDirection += dir;
        OnDirectionChanged(sumOfDirection.normalized);
    }

    public void OnZoomCamera(InputAction.CallbackContext context)
    {
        float scrollY = context.ReadValue<float>();

        if (context.started)
        {
            if (scrollY != 0)
            {
                Vector3 pos = Camera.main.transform.position;
                pos.z += scrollY;
                Camera.main.transform.position = pos;
            }
        }
    }
}
