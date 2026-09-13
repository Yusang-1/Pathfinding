using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitContainer", menuName = "Scriptable Objects/UnitContainer")]
    public class UnitContainerSO : ScriptableObject
    {
        [SerializeField] private Unit[] units;

        private readonly Dictionary<int, Unit> unitDict = new();
        private readonly Dictionary<int, ObjectPool<Unit>> unitPoolDict = new();

        public void Initialize()
        {
            int unitCode;
            for (int i = 0; i < units.Length; i++)
            {
                unitCode = units[i].UnitData.UnitCode;

                if (!unitDict.ContainsKey(unitCode))
                {
                    unitDict.Add(unitCode, units[i]);

                    unitPoolDict.Add(unitCode, new ObjectPool<Unit>());
                }
            }
        }

        public bool TryGetUnit(int unitCode, out Unit unit)
        {
            ObjectPool<Unit> pool;
            if (unitPoolDict.ContainsKey(unitCode))
            {
                pool = unitPoolDict[unitCode];
                if (pool.TryGetObject(out Unit unitInPool))
                {
                    unit = unitInPool;
                    return true;
                }
                else
                {
                    unit = Unit.Instantiate(unitDict[unitCode]);
                    unit.OnPoolObjectUnused += pool.PoolObjectUnused;

                    return true;
                }
            }
            else
            {
                unit = null;
                return false;
            }
        }
    }
}
