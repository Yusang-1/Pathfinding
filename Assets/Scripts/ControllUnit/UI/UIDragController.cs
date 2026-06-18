using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIDragController : MonoBehaviour
    {
        public event Func<Vector3, float, float, HashSet<ISelectableUnit>> OnFindSelectableUnitInDragUI;
        public event Action<HashSet<ISelectableUnit>> OnUnitFocused;

        [SerializeField] private GameObject dragUI;
        
        private HashSet<ISelectableUnit> focusedUnits;
        
        private RectTransform dragUIRect;

        private Vector3 standardPosition;
        private Vector3 compareSizeDelta;

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
            dragUISizeDelta.x = standardPosition.x - position.x;
            dragUISizeDelta.y = standardPosition.y - position.y;
            compareSizeDelta = dragUISizeDelta;

            int x = standardPosition.x < position.x ? 1 : -1;
            int y = standardPosition.y < position.y ? 1 : -1;

            if (x > 0 && y > 0) // 1사분면
            {
                dragUIRect.pivot = new Vector2(0, 0);
                compareSizeDelta.x = Mathf.Abs(dragUISizeDelta.x);
                compareSizeDelta.y = Mathf.Abs(dragUISizeDelta.y);
            }
            else if (x < 0 && y > 0) // 2사분면
            {
                dragUIRect.pivot = new Vector2(1, 0);
                compareSizeDelta.x = -Mathf.Abs(dragUISizeDelta.x);
                compareSizeDelta.y = Mathf.Abs(dragUISizeDelta.y);
            }
            else if (x < 0 && y < 0)  // 3사분면
            {
                dragUIRect.pivot = new Vector2(1, 1);
                compareSizeDelta.x = -Mathf.Abs(dragUISizeDelta.x);
                compareSizeDelta.y = -Mathf.Abs(dragUISizeDelta.y);
            }
            else if (x > 0 && y < 0) // 4사분면
            {
                dragUIRect.pivot = new Vector2(0, 1);
                compareSizeDelta.x = Mathf.Abs(dragUISizeDelta.x);
                compareSizeDelta.y = -Mathf.Abs(dragUISizeDelta.y);
            }

            dragUIRect.sizeDelta = new Vector2(Mathf.Abs(dragUISizeDelta.x), Mathf.Abs(dragUISizeDelta.y));
            
            focusedUnits  = OnFindSelectableUnitInDragUI?.Invoke(standardPosition, compareSizeDelta.x, compareSizeDelta.y);            
            OnUnitFocused?.Invoke(focusedUnits);
        }
        public HashSet<ISelectableUnit> DragCanceled()
        {
            dragUI.SetActive(false);

            return focusedUnits;
        }
    }
}
