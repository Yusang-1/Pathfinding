using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CameraControllerInput input;
    [SerializeField] private float speed;
    
    private Vector3 direction;
    private bool isMoving;

    private void Start()
    {
        input.OnDirectionChanged += GetDirection;
    }
    
    private void Update()
    {
        Move();
    }
    
    public void GetDirection(Vector2 vec)
    {
        if(vec == Vector2.zero)
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
        if(!isMoving) return;
        
        transform.position += direction;
    }
}
