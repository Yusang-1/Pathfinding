using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float speed;
    
    private Vector3 direction;
    private bool isMoving;
    private void Update()
    {
        Move();
    }
    
    public void GetDirection(Vector2 vec)
    {
        isMoving = true;
        
        float value = speed * Time.deltaTime;
        direction.x = vec.x * value;
        direction.y = vec.y * value;
        direction.z = 0;
    }
    
    public void Stop()
    {
        isMoving = false;
    }
    
    private void Move()
    {
        if(!isMoving) return;
        
        transform.position += direction;
    }
}
