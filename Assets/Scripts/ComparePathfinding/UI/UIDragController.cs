using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class UIDragController : MonoBehaviour
    {
        [SerializeField] private GameObject dragUI;

        private RectTransform dragUIRect;

        private Vector3 standardPosition;

        private void Start()
        {
            dragUIRect = dragUI.GetComponent<RectTransform>();
        }

        public void DragStarted(Vector3 startPosition)
        {
            dragUI.SetActive(true);

            standardPosition = startPosition;
            dragUIRect.anchoredPosition = standardPosition;
        }

        private Vector2 dragUISizeDelta;
        public void DragPerformed(Vector3 position)
        {
            dragUISizeDelta.x = Mathf.Abs(standardPosition.x - position.x);
            dragUISizeDelta.y = Mathf.Abs(standardPosition.y - position.y);
            
            int x = standardPosition.x < position.x ? 1 : -1;
            int y = standardPosition.y < position.y ? 1 : -1;            

            if (x > 0 && y > 0) // 1사분면
            {
                dragUIRect.pivot = new Vector2(0, 0);
            }
            else if (x < 0 && y > 0) // 2사분면
            {
                dragUIRect.pivot = new Vector2(1, 0);
            }
            else if (x < 0 && y < 0)  // 3사분면
            {
                dragUIRect.pivot = new Vector2(1, 1);
            }
            else if (x > 0 && y < 0) // 4사분면
            {
                dragUIRect.pivot = new Vector2(0, 1);
            }

            dragUIRect.sizeDelta = dragUISizeDelta;
        }
        public List<ISelectableUnit> DragCanceled()
        {
            dragUI.SetActive(false);
            return null;
        }
    }
}
