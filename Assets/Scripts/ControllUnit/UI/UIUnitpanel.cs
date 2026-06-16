using UnityEngine;
using TMPro;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIUnitpanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI UnitName;

        public void UnitSelected(ISelectableUnit unit)
        {
            GetUnitInfo((unit as Unit).name);
        }
        public void UnitDeselected(ISelectableUnit unit)
        {
            GetUnitInfo(" ");
        }

        private void GetUnitInfo(string name)
        {
            UnitName.text = name;
        }
        
        public void SetActiveTrue() => gameObject.SetActive(true);
    }
}

