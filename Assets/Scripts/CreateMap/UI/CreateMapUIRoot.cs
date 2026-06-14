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

        [SerializeField] private UIGenerateMap uiGenerateMap;
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        [SerializeField] private UIManageMap uiManageMap;
        [SerializeField] private UIPopup uiPopup;

        public void Initialize()
        {
            uiPopup.Initialize();
            
            uiGenerateMap.OnGenerateMap += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            uiGenerateMap.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            uiGenerateMap.OnGenerateMapUI += uiExportMap.SetActiveTrue;
            uiGenerateMap.OnGenerateMapUI += uiManageMap.SetActiveTrue;
            uiGenerateMap.OnGetOfficialMapList += () => OnGetOfficialMapListRequested?.Invoke();
            uiGenerateMap.OnGetPersonalMapList += () => OnGetPersonalMapListRequested?.Invoke();
            uiGenerateMap.OnLoadMap += (mapData) => OnLoadMapRequested?.Invoke(mapData); 
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);

            uiExportMap.OnExprotMap += (mapName) => OnExportMapRequested?.Invoke(mapName);            

            uiManageMap.OnClear += () => OnClearMapRequested?.Invoke();
            uiManageMap.OnRemove += () => OnRemoveMapRequested?.Invoke();
            uiManageMap.OnRemove += uiGenerateMap.SetActiveTrue;
            uiManageMap.OnRemove += uiTileSelector.SetActiveFalse;
            uiManageMap.OnRemove += uiExportMap.SetActiveFalse;
        }
    }
}
