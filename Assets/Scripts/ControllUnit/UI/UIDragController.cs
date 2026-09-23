using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIDragController : MonoBehaviour
    {
        public event Action<Vector3, float, float> OnFindUnitsInDragUI;

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject dragUI;

        private RectTransform dragUIRect;

        private Vector3 standardPosition;
        private Vector3 canvasStandardPosition;
        private Vector3 compareSizeDelta;
        private Vector2 dragUISizeDelta;

        private void Start()
        {
            canvas = canvas != null ? canvas : FindAnyObjectByType<Canvas>();

            dragUIRect = dragUI.GetComponent<RectTransform>();
        }

        public void DragStarted(Vector3 startPosition)
        {
            dragUI.SetActive(true);

            standardPosition = startPosition;
            canvasStandardPosition = ScreenToCanvasPosition(startPosition);
            
            dragUIRect.anchoredPosition = canvasStandardPosition;
        }
        
        public void DragPerformed(Vector3 position)
        {
            var canvasPosition = ScreenToCanvasPosition(position);
            
            // canvas좌표 기준
            dragUISizeDelta = new Vector2(
                canvasStandardPosition.x - canvasPosition.x,
                canvasStandardPosition.y - canvasPosition.y
            );
            
            // screen좌표 기준
            compareSizeDelta = new Vector2(
                standardPosition.x - position.x,
                standardPosition.y - position.y
            );

            DrawDragUI(canvasPosition);

            OnFindUnitsInDragUI?.Invoke(standardPosition, compareSizeDelta.x, compareSizeDelta.y); // screen좌표 기준 전달
        }
        public Vector3 ECSDragPerformed(Vector3 position)
        {
            Camera cam = Camera.main;
            float depth = cam.orthographic ? 0f : Mathf.Abs(cam.transform.position.z);
            Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(position.x, position.y, depth));

            dragUISizeDelta.x = standardPosition.x - position.x;
            dragUISizeDelta.y = standardPosition.y - position.y;

            DrawDragUI(position);

            return worldPosition;
        }

        private void DrawDragUI(Vector3 position)
        {
            int x = canvasStandardPosition.x < position.x ? 1 : -1;
            int y = canvasStandardPosition.y < position.y ? 1 : -1;

            if (x > 0 && y > 0) // 1사분면
            {
                dragUIRect.pivot = new Vector2(0, 0);
                compareSizeDelta.x = Mathf.Abs(compareSizeDelta.x);
                compareSizeDelta.y = Mathf.Abs(compareSizeDelta.y);
            }
            else if (x < 0 && y > 0) // 2사분면
            {
                dragUIRect.pivot = new Vector2(1, 0);
                compareSizeDelta.x = -Mathf.Abs(compareSizeDelta.x);
                compareSizeDelta.y = Mathf.Abs(compareSizeDelta.y);
            }
            else if (x < 0 && y < 0)  // 3사분면
            {
                dragUIRect.pivot = new Vector2(1, 1);
                compareSizeDelta.x = -Mathf.Abs(compareSizeDelta.x);
                compareSizeDelta.y = -Mathf.Abs(compareSizeDelta.y);
            }
            else if (x > 0 && y < 0) // 4사분면
            {
                dragUIRect.pivot = new Vector2(0, 1);
                compareSizeDelta.x = Mathf.Abs(compareSizeDelta.x);
                compareSizeDelta.y = -Mathf.Abs(compareSizeDelta.y);
            }

            dragUIRect.sizeDelta = new Vector2(Mathf.Abs(dragUISizeDelta.x), Mathf.Abs(dragUISizeDelta.y));
        }

        public void DragCanceled()
        {
            dragUI.SetActive(false);
        }

        private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
        {                        
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
            ;
            
            RectTransform parentRect = dragUIRect.parent as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPosition,
                eventCamera,
                out Vector2 localPosition
            );

            return localPosition;
        }
    }
}
