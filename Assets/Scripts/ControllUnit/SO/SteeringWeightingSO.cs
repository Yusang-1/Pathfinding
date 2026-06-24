using System;
using UnityEngine;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "SteeringWeightingSO", menuName = "Scriptable Objects/CrowdSimulation/SteeringWeightingSO")]
    public class SteeringWeightingSO : ScriptableObject
    {
        public SteeringConfig WalkConfig;
    }
    
    [Serializable]
    public struct SteeringConfig
    {
        public float SeekWeight;
        public float SeparationWeight;
        public float CohesionWeight;
        public float AlignmentWeight;
    }
}
