using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class TouchInputBase : MonoBehaviour
{
    private IEnumerator WaitDragCoroutine;

    public bool Touch0Active { get; protected set; }
    public Vector2 Touch0Delta { get; protected set; }
    public Vector2 Touch0Pos { get; protected set; }
    public bool Touch1Active { get; protected set; }
    public Vector2 Touch1Pos { get; protected set; }

    protected bool isPointerOverGameObject;
    public bool IsDrag { get; protected set; }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            isPointerOverGameObject = true;
        }
        else
        {
            isPointerOverGameObject = false;
        }
    }

    public virtual void OnTouch0Contact(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject)
        {
            if (!(context.canceled && IsDrag)) return;
        }

        Touch0Active = context.ReadValueAsButton();

        if (context.started)
        {
            WaitDragCoroutine = WaitDrag(Touch0Pos);
            StartCoroutine(WaitDragCoroutine);
        }

        if (context.performed)
        {
            if (Touch0Delta.sqrMagnitude > 0.02f)
            {
                StopCoroutine(WaitDragCoroutine);
            }
        }

        if (context.canceled)
        {
            if (WaitDragCoroutine != null)
            {
                StopCoroutine(WaitDragCoroutine);
            }            
        }
    }

    private readonly float dragGoalTime = 0.5f;
    private float dragTime = 0;
    private IEnumerator WaitDrag(Vector2 startPosition)
    {
        while (true)
        {
            if (startPosition != Touch0Pos)
            {
                dragTime += Time.deltaTime;

                if (dragTime >= dragGoalTime)
                {
                    HoldStarted();
                    break;
                }
            }
            yield return null;
        }
    }

    private void HoldStarted()
    {
        IsDrag = true;
    }
    
    public void OnTouch0Delta(InputAction.CallbackContext context)
    {
        Touch0Delta = context.ReadValue<Vector2>();
    }

    public virtual void OnTouch0Position(InputAction.CallbackContext context)
    {
        Touch0Pos = context.ReadValue<Vector2>();
    }
}
