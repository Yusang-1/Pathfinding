using UnityEngine;

public class UnitController : MonoBehaviour
{
    private Vector3 direction;
    private bool HasDirection => direction != Vector3.zero;
    
    public void ControllerUpdate()
    {
        Move();
    }
    
    private void Move()
    {
        if(!HasDirection) return;
        
        
    }
    
    public void GetDirection()
    {
        
    }
}
