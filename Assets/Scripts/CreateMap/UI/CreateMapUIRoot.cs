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
        public event Func<CreateMapManager.MapData[]> OnGetOfficialMapListRequested;
        public event Func<CreateMapManager.MapData[]> OnGetPersonalMapListRequested;
        public event Action<CreateMapManager.MapData> OnLoadMapRequested;

        [SerializeField] private UIGenerateMapMediator uiGenerateMapContainer;
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        [SerializeField] private UIManageMap uiManageMap;
        [SerializeField] private UIPopup uiPopup;

        public void Initialize()
        {
            uiPopup.Initialize();
            
            uiGenerateMapContainer.OnGenerateMapRequested += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            
            uiGenerateMapContainer.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            uiGenerateMapContainer.OnGenerateMapUI += uiExportMap.SetActiveTrue;
            uiGenerateMapContainer.OnGenerateMapUI += uiManageMap.SetActiveTrue;
            uiGenerateMapContainer.OnLoadMapRequested += (mapData) => OnLoadMapRequested?.Invoke(mapData);
            uiGenerateMapContainer.OnOfficialMapListRequested += () => OnGetOfficialMapListRequested?.Invoke();
            uiGenerateMapContainer.OnPersonalMapListRequested += () => OnGetPersonalMapListRequested?.Invoke();
            
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);

            uiExportMap.OnExprotMap += (mapName) => OnExportMapRequested?.Invoke(mapName);            

            uiManageMap.OnClear += () => OnClearMapRequested?.Invoke();
            uiManageMap.OnRemove += () => OnRemoveMapRequested?.Invoke();
            uiManageMap.OnRemove += uiGenerateMapContainer.SetActiveTrue;
            uiManageMap.OnRemove += uiTileSelector.SetActiveFalse;
            uiManageMap.OnRemove += uiExportMap.SetActiveFalse;
        }
    }
}
