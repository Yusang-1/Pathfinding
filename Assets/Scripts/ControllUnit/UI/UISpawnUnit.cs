using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISpawnUnit : MonoBehaviour
    {
        public event Action<UnitSize> OnSpawnUnitRequested;
        public event Action<Action> OnGetSpawnAreaRequested;
        public Action OnGetSpawnAreaFinished;
        
        private UnitSize unitSize;
        
        public void OnSpawnUnit()
        {
            OnSpawnUnitRequested?.Invoke(unitSize);
        }
        
        public void OnSetSpawnTypeSmall()
        {
            unitSize = UnitSize.small;
        }
        
        public void OnSetSpawnTypeLarge()
        {
            unitSize = UnitSize.large;
        }

        public void OnSetSpawnArea()
        {
            SetActiveFalse();
            OnGetSpawnAreaFinished += SetActiveTrue;
            OnGetSpawnAreaRequested?.Invoke(OnGetSpawnAreaFinished);
            OnGetSpawnAreaFinished -= SetActiveTrue;
        }

        public void SetActiveTrue() => gameObject.SetActive(true);
        public void SetActiveFalse() => gameObject.SetActive(false);
    }
}
