using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class PlayerControllInput : MonoBehaviour
{    
    public event Action ControllMenu;

    private readonly SelectableController selectableController = new();
    private NodeList nodeList;

    private Vector2 mousePosition;
    private bool isPointerOverGameObject;

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
    
    public void Initialize(NodeList nodeList)
    {
        this.nodeList = nodeList;
    }

    public void OnClick(InputAction.CallbackContext context)
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

    public void OnTrackMousePosition(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }    

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ControllMenu?.Invoke();
        }
    }
}
