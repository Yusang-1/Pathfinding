using UnityEngine;
using System;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISpawnUnit : MonoBehaviour
    {
        public event Action<int> OnSpawnUnitRequested;
        public event Action<Action> OnGetSpawnAreaRequested;
        public Action OnGetSpawnAreaFinished;
        
        public event Action<Action> OnSpawnEvent;
        public event Action<int> OnSpawnUnitCode;
        
        private int unitCode = 101;
        
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
        
        public void OnSpawn()
        {
            SetActiveFalse();
            OnGetSpawnAreaFinished += SetActiveTrue;
            OnSpawnEvent?.Invoke(OnGetSpawnAreaFinished);
            OnSpawnUnitCode?.Invoke(unitCode);
            OnGetSpawnAreaFinished -= SetActiveTrue;
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
