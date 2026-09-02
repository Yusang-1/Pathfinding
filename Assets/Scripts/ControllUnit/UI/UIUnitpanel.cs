using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIUnitpanel : MonoBehaviour
    {
        [SerializeField] UISelectedUnits uiSelectedUnits;
        
        public void UnitSelected(ISelectableUnit unit)
        {
            uiSelectedUnits.SetSelectedUnitInfo(unit);
        }
        public void UnitDeselected(ISelectableUnit unit)
        {
            uiSelectedUnits.DeSelectedUnit(unit);
        }
        
        public void ECSUnitSelected(string name, Entity entity)
        {
            uiSelectedUnits.SetSelectedECSUnitInfo(name, entity);
        }
        
        public void ECSUnitDeSelected(Entity entity)
        {
            uiSelectedUnits.DeselectedECSUnit(entity);
        }
        
        public void SetActiveTrue() => gameObject.SetActive(true);
    }
}

