using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISelectedUnits : MonoBehaviour
    {
        [SerializeField] private UISelectedUnit uiSelectedUnit;
        private RectTransform myRect;

        private UISelectedUnit[] uis;
        private readonly Dictionary<ISelectableUnit, int> uiIndexDict = new();
        private readonly PriorityQueue<UISelectedUnit, int> UnusedUI = new();
        
        private readonly Dictionary<Entity, int> uiIndexHashDict = new();
        private readonly PriorityQueue<UISelectedUnit, int> UnusedUIHashDict = new();

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
                    
                    UnusedUIHashDict.Enqueue(uis[index], index); // ecs unit용 나중에 ecs용 코드는 클래스 이동
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
            
            UISelectedUnit unitUI = UnusedUI.Dequeue();
            unitUI.GetUnitInfo((unit as Unit).name);
            
            uiIndexDict.Add(unit, unitUI.Index);
        }
        public void DeSelectedUnit(ISelectableUnit unit)
        {
            if (!uiIndexDict.ContainsKey(unit))
            {
                Debug.LogWarning("select되지 않은 unit을 deselect 시도");
                return;
            }
            
            UISelectedUnit unitUI = uis[uiIndexDict[unit]];
            unitUI.DeSelect();
            uiIndexDict.Remove(unit);
            
            UnusedUI.Enqueue(unitUI, unitUI.Index);
        }
        
        public void SetSelectedECSUnitInfo(string name, Entity entity)
        {
            if (uiIndexHashDict.ContainsKey(entity))
            {
                Debug.LogWarning("이미 select한 unit을 select 시도");
                return;
            }
            
            UISelectedUnit unitUI = UnusedUIHashDict.Dequeue();
            unitUI.GetUnitInfo(name);
            
            uiIndexHashDict.Add(entity, unitUI.Index);
        }
        
        public void DeselectedECSUnit(Entity entity)
        {
            if (!uiIndexHashDict.ContainsKey(entity))
            {
                Debug.LogWarning("select되지 않은 unit을 deselect 시도");
                return;
            }
            
            UISelectedUnit unitUI = uis[uiIndexHashDict[entity]];
            unitUI.DeSelect();
            uiIndexHashDict.Remove(entity);
            
            UnusedUIHashDict.Enqueue(unitUI, unitUI.Index);
        }
    }
}
