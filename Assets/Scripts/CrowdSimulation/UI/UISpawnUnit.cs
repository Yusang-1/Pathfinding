using UnityEngine;
using System;

namespace Assets.Scripts.CrowdSimulation.UI
{
    public class UISpawnUnit : MonoBehaviour
    {
        public event Action OnSpawnUnitRequested;
        
        public void OnSpawnUnit()
        {
            OnSpawnUnitRequested?.Invoke();
        }
    }
}
