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
        
        [SerializeField] private int nodeSize;
        private int mapSize;
        private int clusterSize;
        
        private void Start()
        {
            nodeList = new NodeList(nodeSize, nodeData);
            
            uiRoot.Initialize(CreateEmptyMap);
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
