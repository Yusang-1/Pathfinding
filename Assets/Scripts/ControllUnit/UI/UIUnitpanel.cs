using UnityEngine;
using Unity.Entities;
using Assets.Scripts.Controller;
using Assets.Scripts.Controller.UI;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UIUnitpanel : MonoBehaviour
    {
        [SerializeField] private UISelectedUnits uiSelectedUnits;
        [SerializeField] private UIForTouchMeditator uiForTouchMeditator;
        private InputStatus inputStatus;
        
        public void AwkaeInitialize(InputStatus inputStatus, ControllScheme controllScheme)
        {
            this.inputStatus = inputStatus;
            
            uiForTouchMeditator.ActiveIfControllSchemeTouch(controllScheme);
        }

        private void OnEnable()
        {
            uiForTouchMeditator.OnShiftPressed += inputStatus.ToggleShift;
        }

        private void OnDisable()
        {
            uiForTouchMeditator.OnShiftPressed -= inputStatus.ToggleShift;
        }

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

