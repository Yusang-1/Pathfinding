using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISelectedUnits : MonoBehaviour
    {
        [SerializeField] private UISelectedUnit uiSelectedUnit;
        private RectTransform myRect;

        private UISelectedUnit[] uis;
        private readonly Dictionary<ISelectableUnit, int> uiIndexDict = new();
        private readonly PriorityQueue<UISelectedUnit, int> UnusedUI = new();
        private int index = 0;

        [SerializeField] private float uiMargin;
        [SerializeField] private int uiRow;
        [SerializeField] private int uiColumn;

        private void Start()
        {
            myRect = GetComponent<RectTransform>();

            SetSelectedUnitList();
        }

        private void SetSelectedUnitList()
        {
            uis = new UISelectedUnit[uiRow * uiColumn];

            float possibleXWidth = (myRect.sizeDelta.x - uiMargin * 2) / uiColumn;
            float possibleYWidth = (myRect.sizeDelta.y - uiMargin * 2) / uiRow;
            float width = possibleXWidth <= possibleYWidth ? possibleXWidth : possibleYWidth;

            float defaultPosX = (myRect.sizeDelta.x - width * uiColumn) / 2;
            float defaultPosY = (myRect.sizeDelta.y - width * uiRow) / 2;

            for (int row = 0; row < uiRow; row++)
            {
                for (int column = 0; column < uiColumn; column++)
                {
                    var ui = Instantiate(uiSelectedUnit, transform);
                    var rect = ui.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(width, width);
                    rect.anchoredPosition = new Vector2(defaultPosX + column * width, -(defaultPosY + row * width));
                    
                    int index = row * uiColumn + column;
                    ui.Initialize(index);
                    ui.gameObject.SetActive(false);
                    uis[index] = ui;
                    
                    UnusedUI.Enqueue(uis[index], index);
                }
            }
        }

        public void SetSelectedUnitInfo(ISelectableUnit unit)
        {
            if (uiIndexDict.ContainsKey(unit))
            {
                Debug.LogWarning("이미 select한 unit을 select 시도");
                return;
            }
            
            var ii = UnusedUI.Dequeue();
            ii.GetUnitInfo((unit as Unit).name);
            
            uiIndexDict.Add(unit, index);
            uis[index++].GetUnitInfo((unit as Unit).name);
        }
        public void DeSelectedUnit(ISelectableUnit unit)
        {
            if (!uiIndexDict.ContainsKey(unit))
            {
                Debug.LogWarning("select되지 않은 unit을 deselect 시도");
                return;
            }
            
            var ii = uis[uiIndexDict[unit]];
            ii.DeSelect();
            
            UnusedUI.Enqueue(ii, ii.Index);
        }
    }
}
