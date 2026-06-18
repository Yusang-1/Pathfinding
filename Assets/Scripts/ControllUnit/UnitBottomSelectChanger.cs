using UnityEngine;
using Assets.Scripts.ControllUnit.SO;

namespace Assets.Scripts.ControllUnit
{
    public class UnitBottomSelectChanger : MonoBehaviour
    {
        [SerializeField] private UnitBottomSelectSO unitBottomSelectSO;
        private SpriteRenderer bottomImage;

        private void Start()
        {
            unitBottomSelectSO.Initialize();
            bottomImage = GetComponent<SpriteRenderer>();
            gameObject.SetActive(false);
        }
        
        public void StatusChanged(UnitBottomStatus status)
        {
            if(status == UnitBottomStatus.None)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                SetSprite(status);
            }
        }
        
        private void SetSprite(UnitBottomStatus status)
        {
            bottomImage.sprite = unitBottomSelectSO.GetSprite(status);
            gameObject.SetActive(true);
        }
    }
    
    public enum UnitBottomStatus
    {
        None,
        Selected,
        Focused
    }
}
