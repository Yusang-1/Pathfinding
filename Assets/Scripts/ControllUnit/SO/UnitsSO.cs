using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitsSO", menuName = "Scriptable Objects/UnitsSO")]
    public class UnitsSO : ScriptableObject
    {
        [SerializeField] private float smallUnitRadius;
        [SerializeField] private float mediumUnitRadius;
        [SerializeField] private float bigUnitRadius;

        public Dictionary<UnitSize, float> UnitRadius;

        public void Initialize()
        {
            UnitRadius = new Dictionary<UnitSize, float>
            {
                { UnitSize.small, smallUnitRadius },
                { UnitSize.medium, mediumUnitRadius },
                { UnitSize.big, bigUnitRadius }
            };
        }
    }    
}

namespace Assets.Scripts.ControllUnit
{
    public enum UnitSize
    {
        small,
        medium,
        big
    }
}
