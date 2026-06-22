using UnityEngine;

namespace Assets.Scripts.CrowdSimulation.SO
{
    [CreateAssetMenu(fileName = "CrowdUnitSO", menuName = "Scriptable Objects/CrowdSimulation/CrowdUnitSO")]
    public class CrowdUnitSO : ScriptableObject
    {
        [SerializeField] private float speed;
        
        public float Speed => speed;
    }
}

