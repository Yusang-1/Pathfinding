using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerController : MonoBehaviour
{
    private Camera thisCamera;

    [Header("Camera Settings")]
    [SerializeField] private float minZoom = 30f;
    [SerializeField] private float maxZoom = 80f;
    [SerializeField] private float speed = 8f;

    [Header("Sensitivity Settings")]
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity;
    private bool isMoving;
    private float currentZoomVelocity;
    private float targetZoom;

    private void Awake()
    {
        thisCamera = GetComponent<Camera>();
        targetZoom = thisCamera.orthographic
            ? thisCamera.orthographicSize
            : thisCamera.fieldOfView;
    }

    private void Update()
    {
        Move();
        UpdateZoom();
    }

    public void SetDirection(Vector2 dir)
    {
        velocity = speed * Time.deltaTime * dir;

        isMoving = velocity != Vector3.zero;
    }

    private void Move()
    {
        if (!isMoving) return;

        transform.position += velocity;
    }

    public void SetTargetZoom(float zoom)
    {
        targetZoom += zoom;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void UpdateZoom()
    {
        if (thisCamera.orthographic)
        {
            thisCamera.orthographicSize = Mathf.SmoothDamp(
                thisCamera.orthographicSize,
                targetZoom,
                ref currentZoomVelocity,
                smoothTime);
        }
        else
        {
            thisCamera.fieldOfView = Mathf.SmoothDamp(
                thisCamera.fieldOfView,
                targetZoom,
                ref currentZoomVelocity,
                smoothTime);
        }
    }
}
