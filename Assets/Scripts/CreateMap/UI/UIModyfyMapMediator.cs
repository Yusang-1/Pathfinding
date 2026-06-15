using UnityEngine;
using System;

namespace Assets.Scripts.CreateMap.UI
{
    public class UIModifyMapMediator : MonoBehaviour
    {
        public event Action<NodeType> OnTileSelectorRequested;
        public event Action<string> OnExportMapRequested;
        public event Action OnClearMapRequested;
        public event Action OnRemoveMapRequested;
        
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        [SerializeField] private UIManageMap uiManageMap;                
                
        public void Initialize()
        {
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);

            uiExportMap.OnExprotMap += (mapName) => OnExportMapRequested?.Invoke(mapName);

            uiManageMap.OnClear += () => OnClearMapRequested?.Invoke();
            uiManageMap.OnRemove += () => OnRemoveMapRequested?.Invoke();            
            uiManageMap.OnRemove += SetActiveFalse;
            
            if(gameObject.activeSelf) SetActiveFalse();
        }
        
        public void SetActiveTrue() => gameObject.SetActive(true);
        public void SetActiveFalse() => gameObject.SetActive(false);
    }
}
