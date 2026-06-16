using System;
using UnityEngine;

namespace Assets.Scripts.ControllUnit.UI
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
