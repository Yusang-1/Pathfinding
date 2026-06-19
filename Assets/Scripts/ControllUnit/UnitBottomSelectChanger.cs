using UnityEngine;
using Assets.Scripts.ControllUnit.SO;
using System;

namespace Assets.Scripts.ControllUnit
{
    public class UnitBottomSelectChanger : MonoBehaviour, IPoolObject<UnitBottomSelectChanger>
    {
        public event Action<UnitBottomSelectChanger> OnPoolObjectFirstCreated;
        public event Action<UnitBottomSelectChanger> OnPoolObjectUnused;
        
        [SerializeField] private UnitBottomSelectSO unitBottomSelectSO;
        private SpriteRenderer bottomImage;        

        public void Initialize()
        {
            unitBottomSelectSO.Initialize();
            bottomImage = GetComponent<SpriteRenderer>();
            OnPoolObjectFirstCreated?.Invoke(this);
            gameObject.SetActive(false);
        }
        public void Despawned()
        {
            OnPoolObjectUnused?.Invoke(this);
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
