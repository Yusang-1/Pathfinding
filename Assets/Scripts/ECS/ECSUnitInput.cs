using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.ECS
{
    public class ECSUnitInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputAction;
        [SerializeField] private PlayerInput playerInputComponent;

        private bool isPointerOverGameObject;

        private void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            InputSingleton.Create(world);
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

        public void OnTrackMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {                
                if (isPointerOverGameObject)
                {
                    InputSingleton.Set(float3.zero, false);
                    return;
                }

                Vector2 mousePosition = context.ReadValue<Vector2>();
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                    new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z)
                );

                InputSingleton.Set(worldPos, true);
            }
        }
    }    
}

