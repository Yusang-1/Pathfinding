using UnityEngine;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISelectedUnits : MonoBehaviour
    {
        [SerializeField] private UISelectedUnit uiSelectedUnit;
        private RectTransform myRect;
        
        private UISelectedUnit[] uis;
        
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
                    ui.gameObject.SetActive(false);
                    uis[row * uiColumn + column] = ui;
                }
            }
        }
    }
}
