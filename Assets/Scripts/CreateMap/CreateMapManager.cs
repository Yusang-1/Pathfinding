using UnityEngine;
using Assets.Scripts.CreateMap.UI;

namespace Assets.Scripts.CreateMap
{
    public class CreateMapManager : MonoBehaviour
    {
        [SerializeField] private Node nodePrefab;
        [SerializeField] private NodeData nodeData;
        [SerializeField] private CreateMapUIRoot uiRoot;                
        
        private NodeList nodeList;
        private readonly MapGenerator mapGenerator = new();
        private readonly NodeTypeController nodeTypeController = new();
        
        [SerializeField] private int nodeSize;
        private int mapSize;
        private int clusterSize;
        
        private void Start()
        {
            nodeData.Initialize();
            
            nodeList = new NodeList(nodeSize, nodeData);
            nodeList.OnSelected += nodeTypeController.SetNodeType;
            
            uiRoot.OnGenerateMapRequested += CreateEmptyMap;
            uiRoot.OnTileSelectorRequested += nodeTypeController.SetCurrentSelected;
            uiRoot.Initialize();
            
            nodeTypeController.Initialize(nodeData);
        }
        
        private void CreateEmptyMap(int sizeOfMap, int sizeOfCluster)
        {
            const int defaultMapSize = 20;
            const int maxClusterSize = 10;
            
            if (sizeOfMap == 0)
            {
                mapSize = defaultMapSize;
            }
            else
            {
                mapSize = sizeOfMap;                
            }

            if (sizeOfCluster == 0)
            {
                clusterSize = mapSize / 4;
                clusterSize = Mathf.Clamp(clusterSize, 0, maxClusterSize);
            }
            else
            {
                clusterSize = sizeOfCluster;
            }
            
            nodeList.CreateNodeArray(mapSize);
            mapGenerator.GenerateMap(mapSize, nodePrefab, nodeList);
        }
    }
}
