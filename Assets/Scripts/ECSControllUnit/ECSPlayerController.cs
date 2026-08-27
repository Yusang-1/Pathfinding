using UnityEngine;

namespace Assets.Scripts.ECSControllUnit
{
    public class ECSPlayerController : MonoBehaviour
    {
        private ECSPlayerControllInput playerControllInput;
        private ECSUnitInput unitInput;

        [SerializeField] private float speed;

        private Vector3 direction;
        private bool isMoving;

        private void Awake()
        {
            playerControllInput = FindAnyObjectByType<ECSPlayerControllInput>();
            unitInput = FindAnyObjectByType<ECSUnitInput>();
        }

        private void OnEnable()
        {
            playerControllInput.OnDirectionChanged += GetDirection;
            unitInput.OnDirectionChanged += GetDirection;
        }

        private void OnDisable()
        {
            playerControllInput.OnDirectionChanged -= GetDirection;
            unitInput.OnDirectionChanged -= GetDirection;
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
