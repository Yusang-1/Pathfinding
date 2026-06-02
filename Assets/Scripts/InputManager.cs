using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Vector2 mousePosition;
    private bool isPointerOverGameObject;
    private readonly SelectableController selectableController = new();

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

    public void OnClick(InputAction.CallbackContext context)
    {
        if (isPointerOverGameObject) return;

        if (context.canceled)
        {
            Vector2 origin = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));

            RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector3.forward, Mathf.Infinity);
            if (hit2D)
            {
                if (hit2D.collider.TryGetComponent<Node>(out Node node))
                {
                    // node.Selected();
                    selectableController.Selected(node);
                }
                else
                    selectableController.Selected(null);
            }
            else
                selectableController.Selected(null);
        }
    }

    public void OnTrackMousePosition(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }

    [SerializeField] private CameraController cameraController;
    public void OnMoveCamera(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            cameraController.GetDirection(context.ReadValue<Vector2>());
        }

        if (context.canceled)
        {
            cameraController.Stop();
        }
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
