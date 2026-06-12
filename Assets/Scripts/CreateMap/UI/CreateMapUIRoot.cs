using System;
using UnityEngine;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {
        public event Action<int, int> OnGenerateMapRequested;
        public event Action<NodeType> OnTileSelectorRequested;
        public event Action<string> OnExportMapRequested;
        
        [SerializeField] private UIGenerateMap uiGenerateMap;
        [SerializeField] private UITileSelector uiTileSelector;
        [SerializeField] private UIExportMap uiExportMap;
        
        public void Initialize()
        {
            uiGenerateMap.OnGenerateMap += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            uiGenerateMap.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            uiGenerateMap.OnGenerateMapUI += uiExportMap.SetActiveTrue;
            
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);
            
            uiExportMap.OnExprotMap += (mapName) => OnExportMapRequested?.Invoke(mapName);
        }
    }
}
