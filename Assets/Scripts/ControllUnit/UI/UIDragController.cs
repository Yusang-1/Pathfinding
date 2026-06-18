using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIDragController : MonoBehaviour
    {
        public event Func<Vector3, float, float, HashSet<ISelectableUnit>> OnFindSelectableUnitInDragUI;
        public event Action<List<ISelectableUnit>> OnUnitFocused;
        public event Action<List<ISelectableUnit>> OnUnitUnfocused;

        [SerializeField] private GameObject dragUI;

        private HashSet<ISelectableUnit> alreadyFocusedHash = new();
        private HashSet<ISelectableUnit> newFocusedHash = new();
        private readonly List<ISelectableUnit> unfocusedUnits = new();
        private readonly List<ISelectableUnit> newlyFocusedUnit = new();
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
            
            newFocusedHash = OnFindSelectableUnitInDragUI?.Invoke(standardPosition, compareSizeDelta.x, compareSizeDelta.y);
            var unfocusedUnits = FindUnfocusedUnits(alreadyFocusedHash, newFocusedHash);
            var newlyFocusedUnit = FindNewlyFocusedUnits(alreadyFocusedHash, newFocusedHash);
            
            alreadyFocusedHash = newFocusedHash;
            
            OnUnitFocused?.Invoke(newlyFocusedUnit);
            OnUnitUnfocused?.Invoke(unfocusedUnits);
        }
        public HashSet<ISelectableUnit> DragCanceled()
        {
            dragUI.SetActive(false);
                        
            return alreadyFocusedHash;
        }
        
        private List<ISelectableUnit> FindUnfocusedUnits(HashSet<ISelectableUnit> alreadyFocused, HashSet<ISelectableUnit> newFocused)
        {
            if(alreadyFocused == null || alreadyFocused.Count == 0) return null;
            if(newFocused == null || newFocused.Count == 0)
            {
                // alreadyFocused Hash의 요소를 unfocusedUnits List에 복사해 리턴
                unfocusedUnits.Clear();
                foreach(var focused in alreadyFocused)
                {
                    unfocusedUnits.Add(focused);
                }
                return unfocusedUnits;
            }
            
            unfocusedUnits.Clear();
            
            foreach(var focused in alreadyFocused)
            {
                if(!newFocused.Contains(focused))
                {
                    unfocusedUnits.Add(focused);
                }
            }
            
            return unfocusedUnits;
        }
        
        private List<ISelectableUnit> FindNewlyFocusedUnits(HashSet<ISelectableUnit> alreadyFocused, HashSet<ISelectableUnit> newFocused)
        {            
            if(newFocused == null || newFocused.Count == 0) return null;
            if(alreadyFocused == null || alreadyFocused.Count == 0)
            {
                // newFocused Hash의 요소를 newlyFocusedUnit List에 복사해 리턴
                newlyFocusedUnit.Clear();
                foreach(var focused in newFocused)
                {
                    newlyFocusedUnit.Add(focused);
                }
                return newlyFocusedUnit;
            }
            
            newlyFocusedUnit.Clear();
            
            foreach(var newFocus in newFocused)
            {
                if(!alreadyFocused.Contains(newFocus))
                {
                    newlyFocusedUnit.Add(newFocus);
                }
            }
            
            return newlyFocusedUnit;
        }
    }
}
