using System;
using UnityEngine;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {
        public event Action<int, int> OnGenerateMapRequested;
        public event Action<NodeType> OnTileSelectorRequested;
        public event Action<string> OnExportMapRequested;
        public event Action OnClearMapRequested;
        public event Action OnRemoveMapRequested;

        [SerializeField] private UIGenerateMap uiGenerateMap;
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        [SerializeField] private UIManageMap uiManageMap;

        public void Initialize()
        {
            uiGenerateMap.OnGenerateMap += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            uiGenerateMap.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            uiGenerateMap.OnGenerateMapUI += uiExportMap.SetActiveTrue;
            uiGenerateMap.OnGenerateMapUI += uiManageMap.SetActiveTrue;

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
