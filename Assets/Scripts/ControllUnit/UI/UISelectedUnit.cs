using UnityEngine;
using TMPro;

namespace Assets.Scripts.ControllUnit.UI
{
    public class UISelectedUnit : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI UnitName;

        private int index;

        public int Index => index;

        public void Initialize(int index)
        {
            this.index = index;
        }
        
        public void GetUnitInfo(string name)
        {
            UnitName.text = name;
            gameObject.SetActive(true);
        }
        public void DeSelect()
        {
            gameObject.SetActive(false);
        }

        public void SetActiveTrue() => gameObject.SetActive(true);
    }
}
