using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class TouchInputBase : MonoBehaviour
{
    protected EventSystem eventSystem;
    protected Touchscreen touchscreen;
    protected IEnumerator JudgeHoldCoroutine;

    public bool Touch0Active { get; protected set; }
    public Vector2 Touch0Delta { get; protected set; }
    public Vector2 Touch0Pos { get; protected set; }
    public bool Touch1Active { get; protected set; }
    public Vector2 Touch1Pos { get; protected set; }

    protected bool isPointerOverGameObject;
    protected bool isJudgingHold;
    public bool IsHold { get; protected set; }

    private void Start()
    {
        eventSystem = EventSystem.current;
        touchscreen = Touchscreen.current;
    }

    private void Update()
    {
        if (Touch0Active)
        {
            CheckTouch0IsOverGameObject();
        }
    }

    protected void CheckTouch0IsOverGameObject()
    {
        int touchId = touchscreen.touches[0].touchId.ReadValue();
        isPointerOverGameObject = eventSystem.IsPointerOverGameObject(touchId);
    }

    public virtual void OnTouch0Contact(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject)
        {
            if (!(context.canceled && IsHold)) return;
        }

        Touch0Active = context.ReadValueAsButton();

        if (context.started)
        {                        
            JudgeHoldCoroutine = WaitDrag(Touch0Pos);
            StartCoroutine(JudgeHoldCoroutine);
        }

        if (context.performed)
        {
            if (Touch0Delta.sqrMagnitude > 0.02f)
            {
                StopCoroutine(JudgeHoldCoroutine);
            }
        }

        if (context.canceled)
        {
            if (JudgeHoldCoroutine != null)
            {
                StopCoroutine(JudgeHoldCoroutine);
            }
        }
    }

    protected readonly float dragGoalTime = 1.2f;
    protected float dragTime = 0;
    protected virtual IEnumerator WaitDrag(Vector2 startPosition)
    {
        isJudgingHold = true;
        while (true)
        {
            if (startPosition != Touch0Pos)
            {
                dragTime += Time.deltaTime;

                if (dragTime >= dragGoalTime)
                {
                    isJudgingHold = false;
                    HoldStarted();
                    break;
                }
            }
            yield return null;
        }
    }

    private void HoldStarted()
    {
        IsHold = true;
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
