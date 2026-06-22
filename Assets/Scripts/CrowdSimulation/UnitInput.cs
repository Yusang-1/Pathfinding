using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Assets.Scripts.CrowdSimulation
{
    public class UnitInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputAction;
        [SerializeField] private PlayerInput playerInputComponent;
        private UnitList unitList;

        private Vector2 mousePosition;
        private bool isPointerOverGameObject;

        private void Start()
        {
            var unitAction = inputAction.actionMaps[2];
            unitAction.Enable();

            playerInputComponent.SwitchCurrentActionMap("Unit");
        }

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

        public void Initialize(UnitList unitList)
        {
            this.unitList = unitList;
        }

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mousePosition = context.ReadValue<Vector2>();

                if (isPointerOverGameObject) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
                unitList.MoveUnits(worldPos);
            }
        }
    }
}
