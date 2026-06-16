using UnityEngine;

namespace Assets.Scripts.ControllUnit.SO
{
    [CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
    public class UnitSO : ScriptableObject
    {
        [SerializeField] private string unitName;
        [SerializeField] private string actionMapName;
        [SerializeField] private SelectableType selectableType;
        
        public string UnitName => unitName;
        public string ActionMapName => actionMapName;
        public SelectableType SelectableType => selectableType;
    }
}
