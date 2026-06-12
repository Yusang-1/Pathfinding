using System;
using UnityEngine;

namespace Assets.Scripts.CreateMap.UI
{
    public class CreateMapUIRoot : MonoBehaviour
    {
        public event Action<int, int> OnGenerateMapRequested;
        public event Action<NodeType> OnTileSelectorRequested;
        
        [SerializeField] private UIGenerateMap uiGenerateMap;
        [SerializeField] private UITileSelector uiTileSelector;
        
        public void Initialize()
        {
            uiGenerateMap.OnGenerateMap += (mapSize, clusterSize) => OnGenerateMapRequested?.Invoke(mapSize, clusterSize);
            uiGenerateMap.OnGenerateMapUI += uiTileSelector.SetActiveTrue;
            
            uiTileSelector.OnTileSelect += (type) => OnTileSelectorRequested?.Invoke(type);
        }
    }
}
