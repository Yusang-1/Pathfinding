using UnityEngine;

namespace Assets.Scripts.ControllUnit
{
    public class MapGenerator
    {
        private readonly Node nodePrefab;
        private readonly NodeList nodeList;

        public MapGenerator(Node prefab, NodeList list)
        {
            nodePrefab = prefab;
            nodeList = list;
        }

        public void GenerateMap(int mapSize)
        {
            nodeList.CreateNodeArray(mapSize);

            Node node;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    node = Node.Instantiate(nodePrefab, new Vector3(i, j, 0), Quaternion.identity);
                    nodeList.SetNode(i, j, node);
                }
            }
        }

        public void GenerateMap(in MapData mapData)
        {
            GenerateMap(mapData.MapSize);

            foreach (var index in mapData.ObstacleIndexes)
            {
                nodeList.NodeTypeController.SetNodeType(index, NodeType.obstacle);
            }
        }
    }
}
