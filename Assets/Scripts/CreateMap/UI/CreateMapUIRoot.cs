using UnityEngine;
using System;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {        
        public event Action<int, int> OnGenerateMapRequested;
        public event Action<NodeType> OnTileSelectorRequested;
        public event Action<string> OnExportMapRequested;
        public event Action OnClearMapRequested;
        public event Action OnRemoveMapRequested;
        public event Func<MapData[]> OnGetOfficialMapListRequested;
        public event Func<MapData[]> OnGetPersonalMapListRequested;
        public event Action<MapData> OnLoadMapRequested;

        [SerializeField] private UIGenerateMapMediator uiGenerateMapMediator;
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        [SerializeField] private UIManageMap uiManageMap;
        [SerializeField] private UIPopup uiPopup;

        public void Initialize()
        {
            uiPopup.Initialize();
            
            uiGenerateMapMediator.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            
            uiGenerateMapMediator.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            uiGenerateMapMediator.OnGenerateMapUI += uiExportMap.SetActiveTrue;
            uiGenerateMapMediator.OnGenerateMapUI += uiManageMap.SetActiveTrue;
            uiGenerateMapMediator.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
            uiGenerateMapMediator.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiGenerateMapMediator.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
            
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);

            uiExportMap.OnExprotMap += (mapName) => OnExportMapRequested?.Invoke(mapName);            

            uiManageMap.OnClear += () => OnClearMapRequested?.Invoke();
            uiManageMap.OnRemove += () => OnRemoveMapRequested?.Invoke();
            uiManageMap.OnRemove += uiGenerateMapMediator.SetActiveTrue;
            uiManageMap.OnRemove += uiTileSelector.SetActiveFalse;
            uiManageMap.OnRemove += uiExportMap.SetActiveFalse;
        }
    }
}
