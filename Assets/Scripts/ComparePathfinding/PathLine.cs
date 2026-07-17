using UnityEngine;
using System;

public class PathLine : MonoBehaviour, IPoolObject<PathLine>
{
    public event Action<PathLine> OnPoolObjectFirstCreated;
    public event Action<PathLine> OnPoolObjectUnused;

    [SerializeField] private GameObject lineHead;

    public void Initialize()
    {
        OnPoolObjectFirstCreated?.Invoke(this);
    }

    public void SetPosition(Vector3 scale, Vector3 startPosition, Vector3 endPosition, Vector3 direction)
    {
        transform.localScale = scale;

        Vector3 position = (startPosition + endPosition) / 2;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
        
        Vector3 headScale = lineHead.transform.localScale;
        if(transform.localScale.y == 0) return;
        
        headScale.y /= transform.localScale.y;
        lineHead.transform.localScale = headScale;
        lineHead.transform.SetPositionAndRotation(endPosition, Quaternion.Euler(0, 0, angle));
        
        gameObject.SetActive(true);
        lineHead.SetActive(true);
    }

    public void ResetLine()
    {
        gameObject.SetActive(false);
        lineHead.SetActive(false);
        OnPoolObjectUnused?.Invoke(this);
    }
}
