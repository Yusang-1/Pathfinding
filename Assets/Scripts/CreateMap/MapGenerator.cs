using UnityEngine;

namespace Assets.Scripts.CreateMap
{
    public class MapGenerator
    {
        private readonly Node nodePrefab;
        private readonly NodeList nodeList;
        private readonly AbstractSpawner unitSpawner;

        private readonly ObjectPool<Node> nodePool = new();

        public MapGenerator(Node prefab, NodeList list, AbstractSpawner spawner)
        {
            nodePrefab = prefab;
            nodeList = list;
            unitSpawner = spawner;
        }

        public void GenerateMap(int mapSize)
        {
            // nodeList.CreateNodeArray(mapSize);

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (!nodePool.TryGetObject(out Node node))
                    {
                        // nodePool에서 가져올게 없다면
                        node = Node.Instantiate(nodePrefab, new Vector3(i, j, 0), Quaternion.identity);
                        node.OnPoolObjectUnused += nodePool.PoolObjectUnused;
                    }
                    else
                    {
                        node.transform.position = new Vector3(i, j, 0);
                    }

                    nodeList.SetNode(i, j, node);
                }
            }
        }

        public void GenerateMap(int mapSize, MapData.Terrain terrainData)
        {
            GenerateMap(mapSize);

            foreach (var index in terrainData.ObstacleIndexes)
            {
                nodeList.NodeTypeController.SetNodeType(index, NodeType.obstacle);
            }
        }

        public void SetUnit(MapData.Unit mapDataUnitPosition)
        {
            if (unitSpawner == null) return;

            for (int index = 0; index < mapDataUnitPosition.UnitCodes.Length; index++)
            {
                unitSpawner.SpawnUnit(mapDataUnitPosition.UnitCodes[index], mapDataUnitPosition.Positions[index]);
            }
        }
    }
}
