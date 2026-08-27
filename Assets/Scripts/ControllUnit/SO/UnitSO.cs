using UnityEngine;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
    public class UnitSO : ScriptableObject
    {
        [SerializeField] private string unitName;
        [SerializeField] private string actionMapName;
        [SerializeField] private SelectableType selectableType;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float refineLength = 2.2f;
        [SerializeField] private UnitSize unitSize;
        [SerializeField] private UnitsSO unitsData;
        
        public string UnitName => unitName;
        public ActionMaps ActionMap => ActionMap;
        public SelectableType SelectableType => selectableType;
        public float MoveSpeed => moveSpeed;
        public float RefineLength => refineLength;
        public float Radius => unitsData.UnitRadius[unitSize];
    }
}
