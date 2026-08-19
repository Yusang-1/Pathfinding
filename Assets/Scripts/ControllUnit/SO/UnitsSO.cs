using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitsSO", menuName = "Scriptable Objects/UnitsSO")]
    public class UnitsSO : ScriptableObject
    {
        [SerializeField] private float smallUnitRadius;
        // [SerializeField] private float mediumUnitRadius;
        [SerializeField] private float largeUnitRadius;
        [SerializeField] private float zeroSizeUnitRadius;

        public Dictionary<UnitSize, float> UnitRadius;

        public void Initialize()
        {
            UnitRadius = new Dictionary<UnitSize, float>
            {
                { UnitSize.small, smallUnitRadius },
                // { UnitSize.medium, mediumUnitRadius },
                { UnitSize.large, largeUnitRadius },
                { UnitSize.zero, zeroSizeUnitRadius }
            };
        }
    }
}

public enum UnitSize
{
    small,
    // medium,
    large,
    zero
}
