using UnityEngine;
using Assets.Scripts.Controller;

namespace Assets.Scripts.ControllUnit
{
    public class PlayerController : MonoBehaviour
    {
        private CameraControllInput cameraControllInput;
        
        [SerializeField] private float speed;

        private Vector3 direction;
        private bool isMoving;

        private void Awake()
        {
            cameraControllInput = FindAnyObjectByType<CameraControllInput>();
        }
        
        private void OnEnable()
        {
            cameraControllInput.OnDirectionChanged += GetDirection;
        }

        private void OnDisable()
        {
            cameraControllInput.OnDirectionChanged -= GetDirection;
        }

        private void Update()
        {
            Move();
        }

        private void GetDirection(Vector2 vec)
        {
            if (vec == Vector2.zero)
            {
                isMoving = false;
                return;
            }

            isMoving = true;

            vec = vec.normalized;
            float value = speed * Time.deltaTime;
            direction.x = vec.x * value;
            direction.y = vec.y * value;
            direction.z = 0;
        }

        private void Move()
        {
            if (!isMoving) return;

            transform.position += direction;
        }
    }
}
