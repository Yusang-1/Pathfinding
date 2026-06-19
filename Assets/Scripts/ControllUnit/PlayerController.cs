using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed;

        private Vector3 direction;
        private bool isMoving;

        private void Start()
        {
            FindAnyObjectByType<PlayerControllInput>().OnDirectionChanged += GetDirection;
            FindAnyObjectByType<UnitInput>().OnDirectionChanged += GetDirection;
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
