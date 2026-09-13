using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISpawnUnit : MonoBehaviour
    {
        public event Action<int> OnSpawnUnitRequested;
        public event Action<Action> OnGetSpawnAreaRequested;
        public Action OnGetSpawnAreaFinished;
        
        private int unitCode;
        
        public void OnSpawnUnit()
        {
            OnSpawnUnitRequested?.Invoke(unitCode);
        }
        
        public void OnSetSpawnTypeSmall()
        {
            unitCode = 101;
        }
        
        public void OnSetSpawnTypeLarge()
        {
            unitCode = 102;
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
